/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Data;
using Limaki.Common;

namespace Limaki.Data {
    
    public class DbGateway : GatewayBase, IDbGateway {

        public DbGateway () { }

        public DbGateway (IDbProvider provider) {
            this._provider = provider;
        }

        IDbProvider _provider = null;
        public virtual IDbProvider Provider {
            get { return _provider ?? (_provider = Registry.Pooled<DbProviderPool> ().Get (Iori.Provider)); }
        }

        public virtual IDbConnection CreateConnection () {
            return Provider.GetConnection (this.Iori);
        }

        protected IDbConnection _connection = null;
        public virtual IDbConnection Connection {
            get {
                if (_connection == null) {
                    _connection = CreateConnection ();
                    _connection.Open ();
                    this.IsClosed = false;
                }
                return _connection;
            }
        }

        protected virtual bool IsGatewayDisposing { get; set; }

        public override void Open (Iori iori) {
            IsGatewayDisposing = false;
            this.Iori = iori;
            this.IsClosed = false;
        }

        public override bool IsOpen { get { return Iori != null && !IsClosed; } }

        public override void Close () {

            if (_connection != null && Connection.State == ConnectionState.Open) {
                Connection.Close ();
                Connection.Dispose ();
            }
            this.IsClosed = true;
            _connection = null;
        }

        public override void Dispose () {
            IsGatewayDisposing = true;
            Close ();
        }

    }

    
}