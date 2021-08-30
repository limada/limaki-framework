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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Limaki.Common;
using Limaki.UnitsOfWork;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.SchemaProvider;

namespace Limaki.Data {

    public class LinqToDBModelBuilder : ILinqToDBModelBuilder {

        public string TablePrefix { get; set; } = "";

        static IDictionary<string, ISet<Type>> _checkedTables = new Dictionary<string, ISet<Type>> ();

        ISet<Type> CheckedTables (DataConnection connection) {
            var key = connection.ConnectionKey ();

            if (key == null)
                return default;

            if (_checkedTables.TryGetValue (key, out var result)) {
                return result;
            }

            result = new HashSet<Type> ();
            _checkedTables.Add (key, result);

            return result;
        }

        static IDictionary<string, ISet<string>> _checkedTableNames = new Dictionary<string, ISet<string>> ();

        ISet<string> CheckedTableNames (DataConnection connection) {
            var key = connection.ConnectionKey ();

            if (key == null)
                return default;

            if (_checkedTableNames.TryGetValue (key, out var result)) {
                return result;
            }

            result = new HashSet<string> ();
            _checkedTableNames.Add (key, result);

            return result;
        }

        public virtual bool CheckTable<T> (DataConnection connection, bool allowCreation) {
            var checkedTables = CheckedTables (connection);

            if (checkedTables.Contains (typeof(T)))
                return true;

            checkedTables.Add (typeof(T));
            var checkedTableNames = CheckedTableNames (connection);
            var tableName = connection.TableName<T> ();

            if (checkedTableNames.Contains (tableName))
                return true;

            checkedTableNames.Add (tableName);

            if (allowCreation) {
                CreateTable<T> (connection, tableName);
            }

            return true;

        }

        static IDictionary<string, DatabaseSchema> _tableSchema = new Dictionary<string, DatabaseSchema> ();

        public static DatabaseSchema GetDatabaseSchema (DataConnection connection) {
            var key = connection.ConnectionKey ();

            if (key == null)
                return null;

            if (!_tableSchema.TryGetValue (key, out var schema)) {
                schema = connection.DatabaseSchema ();
                _tableSchema.Add (key, schema);
            }

            return schema;
        }

        public bool CreateTable<T> (DataConnection connection, string tableName = default) {

            tableName ??= typeof(T).Name.ToLower ();

            var desc = connection.MappingSchema.GetEntityDescriptor (typeof(T));

            var schema = GetDatabaseSchema (connection);

            if (!schema.Tables.Any (t => t.TableName == desc.TableName)) {
                connection.CreateTable<T> (desc.TableName, desc.DatabaseName, null, null, null);

                return true;
            }

            return false;
        }

        static IDictionary<string, ISet<Type>> _checkedModels = new Dictionary<string, ISet<Type>> ();

        ICollection<Type> CheckedModels (Iori iori) => CheckedModels (iori.ConnectionString ());

        ICollection<Type> CheckedModels (string key) {
            if (_checkedModels.TryGetValue (key, out var result)) {
                return result;
            }

            result = new HashSet<Type> ();
            _checkedModels.Add (key, result);

            return result;
        }

        public virtual void CheckModel<T> (DataConnection connection) {

            var entityType = typeof(T);
            var checkedModels = CheckedModels (connection.ConnectionString);

            if (!checkedModels.Contains (entityType)) {

                checkedModels.Add (entityType);

                var builder = connection.MappingSchema.GetFluentMappingBuilder ();
                var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

                var methods = new HashSet<MethodInfo> (
                    //  add methods with TypeGuid
                    this.GetType ().GetMethods ().Where (
                        m => m.GetCustomAttributes<TypeGuidAttribute> ().Any (t => t.Type == entityType)
                             && m.GetParameters ().FirstOrDefault ()?.ParameterType == typeof(LinqToDB.Mapping.FluentMappingBuilder)
                    )
                ) {
                    // add by convention
                    this.GetType ().GetMethod ("Fluent" + entityType.Name, bindingFlags)
                };

                foreach (var method in methods) {
                    method?.Invoke (this, new object[] {builder});
                }

            }
        }

        public bool AllowCreation (Iori iori) => iori != default && iori.AccessMode != IoMode.Client;

        public bool CheckIndices<T> (DataConnection connection) =>
            new LinqToDBIndexBuilder (connection)
               .CheckIndices<T> ();

        public virtual void CheckModel<T> (LinqToDBGateway gateway) {
            CheckModel<T> (gateway.Connection);
        }

        public virtual bool CheckTable<T> (LinqToDBGateway gateway) => CheckTable<T> (gateway.Connection, AllowCreation (gateway.Iori));

        public bool CheckIndices<T> (LinqToDBGateway gateway) => AllowCreation (gateway?.Iori) && CheckIndices<T> (gateway.Connection);

        bool IDbModelBuilder.CheckTable<T> (IGateway gateway) => CheckTable<T> (gateway as LinqToDBGateway);

        bool IDbModelBuilder.CheckIndices<T> (IGateway gateway) => CheckIndices<T> (gateway as LinqToDBGateway);

        void IDbModelBuilder.CheckModel<T> (IGateway gateway) => CheckModel<T> (gateway as LinqToDBGateway);

    }

}