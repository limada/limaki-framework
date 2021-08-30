/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Limaki.Common;
using Limaki.Data;

namespace Limaki.Repository {

    public class InMemoryDbQuoreProvider : IDbProvider {

        private Dictionary<Iori, InMemoryQuore> _stores = new Dictionary<Iori, InMemoryQuore> ();

        public IQuore GetCreateQuore (IDbGateway gateway) {
            if (!_stores.TryGetValue (gateway.Iori, out var result)) {
                var quore = new InMemoryQuore {Gateway = gateway};
                _stores[gateway.Iori] = quore;
                result = quore;
            }
            return result;
        }

        public string Name => "InMemoryProvider";

        public string ConnectionString (Iori iori) {
            return string.Empty;
        }

        public IDbConnection GetConnection (Iori iori) {
            return new InMemoryQuoreDbConnection ();
        }

        public bool CreateDatabase (Iori iori) {
            return true;
        }

        public bool DropDatabase (Iori iori) {
            return true;
        }

        public bool DataBaseExists (Iori iori) {
            return true;
        }

        public bool CloseEverything () {
            return true;
        }
    }


    public class InMemoryQuoreDbConnection : DbConnection {


        protected override DbTransaction BeginDbTransaction (System.Data.IsolationLevel isolationLevel) {
            throw new NotImplementedException ();
        }

        public override void ChangeDatabase (string databaseName) {}

        public override void Close () {
         
        }

        public override string ConnectionString { get; set; }
         

        protected override DbCommand CreateDbCommand () {
            throw new NotImplementedException ();
        }

        public override string DataSource {
            get { return string.Empty; }
        }

        public override string Database {
            get { return string.Empty; }
        }

        public override void Open () {
            
        }

        public override string ServerVersion {
            get { return string.Empty; }
        }

        public override System.Data.ConnectionState State {
            get { return System.Data.ConnectionState.Open; }
        }
    }
}