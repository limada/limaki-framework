/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Linq.Expressions;
using Limaki.Common;

namespace Limaki.UnitsOfWork {
    /// <summary>
    /// Service facade.
    /// used to consume a servcice on client side
    /// </summary>
    public abstract class ClientUsecase<S, M> : IClientUsecase where S : IServiceBase where M : StoreManager, new() {

        public ClientUsecase (S service) {
            Service = service;
        }

        ILog _log = null;
        protected virtual ILog Log => _log ?? (_log = Registry.Pool.TryGetCreate<Logger> ().Log (this.GetType ()));

        M _storeManager = null;
        public M StoreManager => _storeManager ?? (_storeManager = new M { Log = Log });

        public Store Store { get => StoreManager.Store; }

        public virtual void SaveTo (ChangeSetContainer container) => StoreManager.SaveTo (container);

        public Action<Action> Caller { get; set; }

        public abstract string ServiceInfo ();

        public abstract void SaveChanges ();

        public abstract void SaveChanges (Store store);

        public abstract void Connect ();

        public abstract void LoadOnStart ();

        public abstract void ClearOnEnd ();

        #region Service

        protected S Service { get; set; }

        public T ServiceCall<T> (Func<S, T> call) {
            try {
                
                if (Caller != null) {
                    var result = default (T);
                    Caller (() => result = call (Service));
                    return result;
                } else {
                    return call (Service);
                }
            } catch (Exception ex) {
                Catch (ex);
                return default (T);
            }
        }

        public void ServiceCall (Action<S> call) {
            try {
                if (Caller != null) {
                    Caller (() => call (Service));
                } else {
                    call (Service);
                }
            } catch (Exception ex) {
                Catch (ex);
            }
        }

        public Type ServiceType {
            get {
                try {
                    return Service?.GetType ();
                } catch (Exception ex) {
                    Catch (ex);
                    return null;
                }
            }
        }

        protected virtual void Catch (Exception ex) {
            Log.Error ($"{ex.GetType ()}:{ex.Message}");
            throw ex;
        }

        #endregion
    }

}
