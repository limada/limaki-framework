/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using Limaki.Common.Collections;

namespace Limaki.Common.UnitsOfWork {
  
    public class Store:IDisposable {
        public Store() {
            Instrument ();
        }

        #region Factory
        public virtual IFactory ItemFactory { get; set; }
        
        public virtual T Create<T>() {
            return ItemFactory.Create<T>();
        }
        public virtual T Create<T>(params object[] args) {
            return ItemFactory.Create<T>(args);
        }
        #endregion

        #region IdentityMap

        bool _identityMapOwner = false;
        IdentityMap _identityMap = null;
        public IdentityMap IdentityMap {
            get {
                if(_identityMap==null){
                    _identityMap = new IdentityMap();
                    _identityMapOwner = true;
                }
                _identityMap.ItemFactory = this.ItemFactory;
                return _identityMap;
            }
            set {
                _identityMap = value;
                _identityMapOwner = false;
            }
        }

        public bool Add<T>(T item) {
            return IdentityMap.Add<T>(item);
        }
        
        public bool Refresh<T>(T item) {
            return IdentityMap.Refresh<T>(item);
        }

        public T Unique<T>(T item) {
            return IdentityMap.Unique<T>(item);
        }

        public T Item<T,TKey>(TKey key) {
            return IdentityMap.Item<T, TKey>(key);
        }

        public T Item<T>(Func<T, bool> predicate) {
            return IdentityMap.Item<T>(predicate);
        }

        #endregion

        #region State

        bool _stateowner = false;
        StateMap _state = null;
        public virtual StateMap State {
            get {
                if (_state == null) {
                    _state = new StateMap();
                    _stateowner = true;
                }
                _state.ItemFactory = this.ItemFactory;
                return _state;
            }
            set {
                _state = value;
                _stateowner = false;
            }
        }
        
        public ICollection<T> Created<T>() {
            return State.Created<T>();
        }
        public ICollection<T> Updated<T>() {
            return State.Updated<T>();
        }

        public ICollection<T> Removed<T>() {
            return State.Removed<T>();
        }

        public void AddCreated<T>(T item) {
            if (item != null) {
                State.AddCreated(item);
                IdentityMap.Add(item);
            }
        }

        public void Update<T>(T item) {
            if (item != null) {
                State.Update(item);
                //Collection<T>(created).Remove(item);
            }
        }

        public void Remove<T>(T item) {
            if (item != null) {
                State.Remove(item);
                IdentityMap.Remove(item);
            }
        }

        public int ChangeCount() {
            return State.ChangeCount();
        }

        public virtual void ClearChanges() {
            if (_state != null) {
                _state.ClearChanges();
                if (_stateowner)
                    _state.Dispose();
            }
            _state = null;
        }

        #endregion

        protected virtual void Instrument() { }

         public virtual void Dispose(bool disposing) {
             ClearChanges();
             if (_identityMap != null)
                 if (_identityMapOwner)
                     IdentityMap.Dispose();
                 else
                     IdentityMap.Clear();

            if (disposing) {
                ItemFactory = null;
            }
        }

        public virtual void Dispose() {
            Dispose(true);
        }

        ~Store() {
            Dispose(false);
        }
    }
}