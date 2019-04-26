using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Limaki.Data {

    public class InMemoryDbQuoreProvider : IDbProvider {

        private Dictionary<Iori, InMemoryQuore> _stores = new Dictionary<Iori, InMemoryQuore> ();
        public IQuore GetCreateQuore (Iori iori) {
            var result = default(InMemoryQuore);
            if (!_stores.TryGetValue (iori, out result)) {
                result = new InMemoryQuore ();
                _stores[iori] = result;
            }
            return result;
        }

        public string Name {
            get { return "InMemoryProvider"; }
        }

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