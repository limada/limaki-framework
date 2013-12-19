/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2008-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;

namespace Limaki.Common.Services {
    public class ClientContext<I> : IDisposable {
        protected I _service = default(I);
        public virtual I Service {
            get {
                if (_service == null) {
                    Open();
                    _service = _client.Service;
                }
                return _service;
            }
        }

        protected ClientHost<I> _client = null;
        public ClientHost<I> Client {
            get {
                if (_client == null) {
                    Open();
                }
                return _client;
            }
        }

        public virtual IServiceSettings Settings { get; set; }

        public virtual void Open() {
            if (_client == null) {
                _client = new ClientHost<I>{Settings = this.Settings};
                _client.Open();
            }
        }

        public virtual void Close() {
            if (_client != null) {
                _client.Close();
                _client = null;
                _service = default(I);
            }
        }

        public virtual void Abort () {
            if (_client != null) {
                _client.Abort();
                _client = null;
                _service = default(I);
            }
        }

        public virtual void Dispose(bool disposing) {
            if (disposing)
                Close();
            _client = null;
            _service = default(I);
        }

        public virtual void Dispose() {
            this.Dispose(true);
        }

        ~ClientContext() {
            this.Dispose(false);
        }

    }

    public class ClientContextVariance<I> : IDisposable {
        public ClientContextVariance(IDisposable client, I service) {
            this.Service = service;
            this.Client = client;
        }

        public I Service { get; protected set; }
        public Action Disposed { get; set; }
        protected IDisposable Client { get; set; }
        
        public void Dispose() {
            Client.Dispose();
            Client = null;
            Service = default(I);
            if (Disposed != null)
                Disposed();
        }
    }
}