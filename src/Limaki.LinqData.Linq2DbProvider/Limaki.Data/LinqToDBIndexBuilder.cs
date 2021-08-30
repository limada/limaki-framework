/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections;
using System.Linq;
using LinqToDB.Data;
using LinqToDB;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Reflection;
using Limaki.Common;
using Limaki.Common.Reflections;
using LinqToDB.Mapping;

namespace Limaki.Data {

    public class LinqToDBIndexBuilder {

        public LinqToDBIndexBuilder (DataConnection connection) {
            Connection = connection;
        }

        public DataConnection Connection { get; }

        public string ProviderName => Connection?.DataProvider.Name;

        public string IndexName (string tableName, params string[] columnNames) => $"idx_{tableName}_{string.Join ("_", columnNames)}";

        public string CreateIndexCommandText (string tableName, params string[] columnNames) {

            // ms sql server: CREATE INDEX i1 ON t1 (col1);
            // sqlite: CREATE INDEX 'i1' ON 't1' ('col1');
            // postgres: CREATE INDEX "indexname" ON "tablename" USING btree("columnname");

            var idxName = IndexName (tableName, columnNames);
            var idxColumnPrefix = "";
            var delim = "";

            if (ProviderName.ToLower ().Contains ("postgres")) {
                delim = "\"";
                idxColumnPrefix = "USING btree";
            }

            if (ProviderName.ToLower ().Contains ("sqlserver")) {
                delim = "\"";
            }

            var columns = string.Join (",", columnNames.Select (columnName => $"{delim}{columnName}{delim}"));

            return $"CREATE INDEX {delim}{idxName}{delim} ON {delim}{tableName}{delim}  {idxColumnPrefix} ({columns})";
        }

        IEnumerable<(PropertyInfo property, IndexAttribute[] attributes)> PropertyIndices (
            Type type,
            Func<(PropertyInfo property, IndexAttribute[] attributes), (PropertyInfo property, IndexAttribute[] attributes)> takeIt = null) {
            var schema = Connection.MappingSchema;

            return
                type.GetProperties ()
                   .Select (property => (property, schema.GetAttributes<IndexAttribute> (type, property)))
                   .Select (pi => takeIt != null ? takeIt (pi) : pi)
                   .Where (pi => (pi != default) && (pi.Item2?.Any () ?? false))
                   .ToArray ()
                ;
        }

        IEnumerable<(PropertyInfo property, IndexAttribute[] attributes)> PropertySingleIndices (Type type)
            => PropertyIndices (type, pi => (
                pi.property,
                pi.attributes.Where (a => string.IsNullOrEmpty (a.Name) || a.Order == -1).ToArray ())
            );

        IEnumerable<DataRow> IndexColumns (string tableName) {
            return ((DbConnection) Connection.Connection).GetSchema ("IndexColumns").AsEnumerable ()
               .Where (r => r.Field<string> ("TABLE_NAME") == tableName);
        }

        IEnumerable<(IndexAttribute attribute, PropertyInfo[] properties)> PropertyMultiIndices (Type type) {
            var idx = PropertyIndices (type, pi => (
                pi.property,
                pi.attributes.Where (a => !string.IsNullOrEmpty (a.Name) && a.Order != -1).ToArray ()
            ));

            return idx
               .SelectMany (pi => pi.attributes.Select (a => new {attribute = a, property = pi.property}))
               .GroupBy (ap => ap.attribute.Name)
               .Select (g => (
                    g.First ().attribute,
                    g.OrderBy (ap => ap.attribute.Order).Select (ap => ap.property).ToArray ())
                )
               .ToArray ();
        }

        public IEnumerable<string> CreateIndexCommandTexts<T> (string tableName = null) {

            var entityType = typeof(T);
            var result = new Queue<string> ();

            try {

                if (tableName == null)
                    tableName = TableName<T> ();

                foreach (var idx in PropertySingleIndices (entityType)) {
                    result.Enqueue (CreateIndexCommandText (tableName, idx.property.Name));
                }

                foreach (var idx in PropertyMultiIndices (entityType)) {
                    result.Enqueue (CreateIndexCommandText (tableName, idx.properties.Select (p => p.Name).ToArray ()));
                }

            } catch (Exception ex) {
                Trace.TraceError ($"Error in {nameof(CreateIndexCommandTexts)}<{typeof(T).Name}>:{ex.Message}");

                throw;
            }

            return result;
        }

        public string TableName<T> () => TableName (typeof(T));

        public string TableName (Type entityType) => Connection.MappingSchema.GetEntityDescriptor (entityType).TableName;

        public void CreateIndex<T> (params string[] columnNames) {
            CreateIndex (typeof(T), columnNames);
        }

        string[] ColumnNames (Type type, params string[] columnNames) {
            return columnNames.Select (s => type.GetProperties ().FirstOrDefault (p => p.Name == s).GetCustomAttribute<ColumnAttribute> ()?.Name ?? s)
               .ToArray ();
        }

        public void CreateIndex (Type type, params string[] columnNames) {
            var tableName = TableName (type);
            columnNames = ColumnNames (type, columnNames);
            var idxName = IndexName (tableName, columnNames);

            if (IndexColumns (tableName).All (r => r.Field<string> ("INDEX_NAME").ToLower () != idxName.ToLower ())) {

                var comm = Connection.Connection.CreateCommand ();
                comm.CommandText = CreateIndexCommandText (tableName, columnNames);

                try {
                    comm.ExecuteScalar ();
                } catch (Exception ex) {
                    Trace.TraceError ($"{nameof(CreateIndex)}<{type.Name}> ({string.Join (",", columnNames)}) failed: {ex.Message}");
                }
            }
        }

        public bool SwitchIndices<T> (bool on) {

            if (ProviderName.ToLower ().Contains ("sqlserver")) {

                var tableName = TableName<T> ();
                var comm = Connection.Connection.CreateCommand ();
                var command = new StringBuilder ();

                foreach (var idx in IndexColumns (tableName)) {
                    var idxName = idx.Field<string> ("INDEX_NAME");

                    if (idxName.StartsWith ("PK_"))
                        continue;

                    command.AppendLine ($"ALTER INDEX {idxName} ON {tableName} {(on ? "REBUILD" : "DISABLE")}; ");
                }

                if (command.Length == 0)
                    return false;

                comm.CommandText = command.ToString ();
                comm.CommandTimeout = 0;

                //    comm.CommandText = $"ALTER INDEX ALL ON {tableName} {(on ? "REBUILD" : "DISABLE")}; ";

                try {
                    comm.ExecuteScalar ();

                    return true;
                } catch (Exception ex) {
                    Trace.TraceError ($"{nameof(SwitchIndices)}<{typeof(T).Name}> failed: {ex.Message}");

                    return false;
                }
            }

            return false;
        }

        public void CheckForeignKeyIndices<T> () {
            CheckForeignKeyIndices (typeof(T));
        }

        /// <summary>
        /// integrate automatic foreign key handling
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>seealso: https://github.com/linq2db/linq2db/issues/1789</remarks>
        public void CheckForeignKeyIndices (Type type) {

            var thisDescr = Connection.MappingSchema.GetEntityDescriptor (type);

            var associations = thisDescr.Associations
               .Where (a => (a.MemberInfo is PropertyInfo info)
                            && !typeof(IEnumerable).IsAssignableFrom (info.PropertyType)
                            && a.ThisKey.Length == 1 && a.OtherKey.Length == 1);

            foreach (var ass in associations) {
                var info = (PropertyInfo) ass.MemberInfo;
                var fkType = info.PropertyType;
                var fkTypeDescr = Connection.MappingSchema.GetEntityDescriptor (fkType);
                var fkPK = fkTypeDescr.Columns.FirstOrDefault (s => s.IsPrimaryKey);

                if (fkPK != null) {
                    CreateIndex (type, ass.ThisKey);
                }
            }

        }

        public void CheckRelationIndices<T> () {
            CheckRelationIndices (typeof(T));
        }

        private string[] _definedTypes = null;

        protected string[] DefinedTypes => _definedTypes ??= Connection.TableSchemas ().Select (s => s.TypeName).ToArray ();

        public void CheckRelationIndices (Type type) {

            var props = type.GetProperties ();

            void TryCreateIndex (Type t, PropertyInfo p) {
                if (p == null) return;

                if (p.PropertyType.IsSimpleOrNullable ()) {
                    CreateIndex (t, p.Name);
                }
            }

            foreach (var property in props.Where (p => p.IsRelation ())) {
                var relation = property.Relation ().Adjust (property);

                TryCreateIndex (type, relation.ThisKey == property.Name ? property : props.FirstOrDefault (p => p.Name == relation.ThisKey));

                if (DefinedTypes.Contains (TableName (relation.OtherType))) {

                    TryCreateIndex (relation.OtherType, relation.OtherType?.GetProperties ().FirstOrDefault (p => p.Name == relation.OtherKey && !p.IsRelation ()));

                }
            }
        }

        ICollection<Type> CheckedIndices (DataConnection connection) => CheckedIndices (connection.ConnectionKey ());

        static IDictionary<string, ISet<Type>> _checkedIndices = new Dictionary<string, ISet<Type>> ();

        ICollection<Type> CheckedIndices (string key) {
            if (_checkedIndices.TryGetValue (key, out var result)) {
                return result;
            }

            result = new HashSet<Type> ();
            _checkedIndices.Add (key, result);

            return result;
        }

        public bool CheckIndices<T> () => CheckIndices (typeof(T));

        public bool CheckIndices (Type entityType) {
            var checkedIndices = CheckedIndices (Connection);

            if (!checkedIndices.Contains (entityType)) {

                try {

                    // disabled. TODO: get a timeout
                    // needs to check all models before!
                    // CheckForeignKeyIndices (entityType);
                    //
                    // CheckRelationIndices (entityType);

                    foreach (var idx in PropertySingleIndices (entityType)) {
                        CreateIndex (entityType, idx.property.Name);
                    }

                    foreach (var idx in PropertyMultiIndices (entityType)) {
                        CreateIndex (entityType, idx.properties.Select (p => p.Name).ToArray ());
                    }
                } catch (Exception ex) {
                    Trace.TraceError ($"Error in {nameof(CheckIndices)}<{entityType.Name}>:{ex.Message}");

                    return false;
                } finally {
                    checkedIndices.Add (entityType);
                }

                return true;
            }

            return false;
        }

    }

}