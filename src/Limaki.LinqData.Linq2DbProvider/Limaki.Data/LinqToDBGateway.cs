/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Linq;
using LinqToDB.Data;
using LinqToDB;
using System.Collections.Generic;
using LinqToDB.SchemaProvider;
using Limaki.Common;
using System.Text;
using LinqToDB.Mapping;

namespace Limaki.Data {

    public class LinqToDBGateway : GatewayBase, IDbGateway, IGatewayExtended {

        protected static string ProviderVersion = "";

        public LinqToDBGateway (IDbProvider provider) {
            _provider = provider as LinqToDBProvider;
        }

        public LinqToDBGateway (Iori iori) {
            Open (iori);
        }

        public override void Open (Iori iori) {
            IsGatewayDisposing = false;
            Iori = iori;
            IsClosed = false;
        }

        LinqToDBProvider _provider = null;

        public virtual LinqToDBProvider Provider {
            get {
                if (_provider == null) {
                    _provider = Registry.Pooled<DbProviderPool> ().Get (Iori.Provider) as LinqToDBProvider;

                    if (_provider == null) {
                        throw new ArgumentException ($"{nameof(LinqToDBGateway)}: provider not found {Iori.Provider}");
                    }

                }

                return _provider;
            }
        }

        IDbProvider IDbGateway.Provider => this.Provider;

        protected virtual bool IsGatewayDisposing { get; set; }

        public override bool IsOpen => Iori != null && !IsClosed;

        public virtual DataConnection CreateConnection () => Provider.GetDataConnection (Iori);

        protected DataConnection _connection = null;

        public virtual DataConnection Connection {
            get {
                if (_connection == null) {
                    _connection = CreateConnection ();
                    IsClosed = false;
                }

                return _connection;
            }
        }

        public override void Close () {
            _connection?.Close ();
            IsClosed = true;
            _connection = null;
        }

        public override void Dispose () {
            IsGatewayDisposing = true;
            Close ();
        }

        public IEnumerable<TableSchema> Tables => Connection.TableSchemas ();

        IEnumerable<string> IGatewayExtended.Tables => Tables.Select (t => t.TableName);

        public bool CreateTable<T> (string tableName = null) {

            var b = GetModelBuilder ();

            return b.AllowCreation (Iori) && b.CreateTable<T> (Connection);

        }

        public string CreateTableStatement<T> (string tableName = null, bool withIndices = false) {

            var desc = Connection.MappingSchema.GetEntityDescriptor (typeof(T));

            if (tableName == null)
                tableName = desc.TableName;

            var sql = new StringBuilder (Connection.CreateTableStatement<T> (tableName, desc.DatabaseName, null, null, null));
            sql.AppendLine (";");

            if (withIndices) {
                foreach (var idx in CreateIndexCommandTexts<T> (tableName)) {
                    sql.Append (idx);
                    sql.AppendLine (";");
                }
            }

            return sql.ToString ();
        }

        public string TableName<T> () => Connection.TableName<T> ();

        public string TableName (Type entityType) => Connection.TableName (entityType);

        public LinqToDBIndexBuilder GetIndexBuilder () => new LinqToDBIndexBuilder (Connection);

        public LinqToDBModelBuilder GetModelBuilder () => new LinqToDBModelBuilder ();

        public string CreateIndexCommandText (string tableName, params string[] columnNames) =>
            GetIndexBuilder ().CreateIndexCommandText (tableName, columnNames);

        public IEnumerable<string> CreateIndexCommandTexts<T> (string tableName = null) =>
            GetIndexBuilder ().CreateIndexCommandTexts<T> (tableName);

        public void CreateIndex<T> (string columnName) => GetIndexBuilder ().CreateIndex<T> (columnName);

        public bool SwitchIndices<T> (bool on) => GetIndexBuilder ().SwitchIndices<T> (on);

        public long BulkCopy<T> (IEnumerable<T> source, Func<long, DateTime, bool> rowsCopied = null) where T : class {

            var options = new BulkCopyOptions {KeepIdentity = true};

            if (Connection.DataProvider.Name == ProviderName.Firebird) {
                //options.MaxBatchCommandSize = 1024 * 60;
                options.MaxBatchSize = 100;
                // options.BulkCopyType = BulkCopyType.RowByRow;
            }

            if (Connection.DataProvider.Name.ToLower ().Contains ("sqlserver")) {
                options.UseInternalTransaction = true;
                options.BulkCopyTimeout = 0;
            }

            if (Connection.DataProvider.Name.ToLower ().Contains ("oracle")) {
                options.UseInternalTransaction = true;
                options.BulkCopyTimeout = 0;
            }

            if (rowsCopied != null) {
                options.NotifyAfter = options.MaxBatchSize ?? 1024;
                options.RowsCopiedCallback = copied => copied.Abort = rowsCopied (copied.RowsCopied, copied.StartTime);
            }

            var result = Connection.BulkCopy (options, source);

            return result.RowsCopied;
        }

    }

    public static partial class DataExtensions { }

}