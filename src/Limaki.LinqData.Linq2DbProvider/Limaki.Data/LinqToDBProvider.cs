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
using System.Data;
using Limaki.Common;
using LinqToDB.Data;

namespace Limaki.Data {

    public class LinqToDBProvider : IDbProvider {

        public LinqToDBProvider (string engine) {
            _engine = engine;
        }

        public static string ProviderSuffix = "LinqToDB.";
        string _engine = null;
        public string Name => $"{ProviderSuffix}{_engine}";

        public Iori PrepareIori (Iori iori) { 
            var result =  Iori.Clone (iori);
            result.Provider = iori.Provider.Replace (ProviderSuffix, "");
            return result;
        }

        public string ProviderVersion { get; protected set; }

        public string ConnectionString (Iori iori) {
            var pIori = PrepareIori (iori);
            var connStr = pIori.ConnectionString ();
            if (pIori.Provider == LinqToDB.ProviderName.SQLite) {
                var pagesize = 4096 * 8;
                //connStr = $"{connStr}; Page Size={pagesize};"; //DateTimeFormat=Ticks; has errors
#if __MonoCS__
                // ProviderVersion = Mono.Data.Sqlite.SqliteConnection.SQLiteVersion;
#endif
            }
            return connStr;
        }

        public DataConnection GetDataConnection (Iori iori) {
            var pIori = PrepareIori (iori);
            var connStr = ConnectionString (iori);
            var conn = new DataConnection (pIori.Provider, connStr) { CommandTimeout = 60 * 3 };
            return conn;

        }

        public IDbConnection GetConnection(Iori iori) => GetDataConnection(iori).Connection;

        public bool DataBaseExists (Iori iori) {
            throw new NotImplementedException ();
        }

        public bool CreateDatabase (Iori iori) {
	        throw new NotImplementedException ();
        }

        public bool DropDatabase (Iori iori) {
            throw new NotImplementedException ();
        }

        public bool CloseEverything () {
            throw new NotImplementedException ();
        }


    }
    
}
