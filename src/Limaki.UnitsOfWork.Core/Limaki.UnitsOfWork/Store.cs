/*
 * Limaki 
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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Limaki.Common;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {

    public class Store : IDisposable {

        public Store () {
            Instrument ();
        }

        #region Factory

        public virtual IFactory ItemFactory { get; set; }

        public virtual T Create<T> () => ItemFactory.Create<T> ();

        public virtual T Create<T> (params object[] args) => ItemFactory.Create<T> (args);

        #endregion

        #region IdentityMap

        bool _identityMapOwner = false;
        IdentityMap _identityMap = null;
        public IdentityMap IdentityMap {
            get {
                if (_identityMap == null) {
                    _identityMap = new IdentityMap ();
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

        public bool Add<T> (T item) => IdentityMap.Add<T> (item);

        public bool Refresh<T> (T item) => IdentityMap.Refresh<T> (item);

        public T Unique<T> (T item) => IdentityMap.Unique<T> (item);

        public T Item<T, TKey> (TKey key) => IdentityMap.Item<T, TKey> (key);

        public T Item<T> (Func<T, bool> predicate) => IdentityMap.Item<T> (predicate);

        public IEnumerable<T> Items<T> () => IdentityMap.Stored<T> ();

        #endregion

        #region State

        bool _stateowner = false;
        StateMap _state = null;
        public virtual StateMap State {
            get {
                if (_state == null) {
                    _state = new StateMap ();
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

        public ICollection<T> Created<T> () => State.Created<T> ();

        public ICollection<T> Updated<T> () => State.Updated<T> ();

        public ICollection<T> Removed<T> () => State.Removed<T> ();

        public T AddCreated<T> (T item) {
            if (item != null) {
                State.AddCreated (item);
                IdentityMap.Add (item);
            }
            return item;
        }

        public virtual T Update<T> (T item) {
            if (item != null) {
                State.Update (item);
                //Collection<T>(created).Remove(item);
            }
            return item;
        }

        public T Remove<T> (T item) {
            if (item != null) {
                State.Remove (item);
                IdentityMap.Remove (item);
            }
            return item;
        }

        public int ChangeCount () => State.ChangeCount ();

        public virtual void ClearChanges () {
            if (_state != null) {
                _state.ClearChanges ();
                if (_stateowner)
                    _state.Dispose ();
            }
            _state = null;
        }

        #endregion

        #region validation

        bool _validatorowner = false;
        Validator _validator = null;
        public virtual Validator Validator {
            get {
                if (_validator == null) {
                    _validator = new Validator ();
                    _validatorowner = true;
                }
                return _validator;
            }
            set {
                _validator = value;
                _validatorowner = false;
            }
        }

        public void AddValidation<T, M> (Expression<Func<T, M>> member, Func<T, M, M, string, bool> validate) => Validator.Add (member, validate);

        public void AddValidation<T, M> (string member, Func<T, M, M, string, bool> validate) => Validator.Add (validate, member);

        public bool IsValidChange<T, M> (Expression<Func<T, M>> member, T item, M oldValue, M newValue) => Validator.IsValidChange (member, item, oldValue, newValue);

        public bool IsValidChange<T, V> (string member, T item, V oldValue, V newValue) => Validator.IsValidChange (item, oldValue, newValue, member);

        public void AddValidation<T, M> (Expression<Func<T, M>> member, Action<T, M, M, string> memberChanged) => Validator.Add (member, memberChanged);

        public void AddValidation<T, M> (string member, Action<T, M, M, string> memberChanged) => Validator.Add (memberChanged, member);

        public void AddValidation<T> (Action<T, string> entityChanged) => Validator.Add (entityChanged);

        public void MemberChanged<T, M> (Expression<Func<T, M>> member, T item, M oldValue, M newValue) => Validator.MemberChanged (member, item, oldValue, newValue);

        public void MemberChanged<T, V> (string member, T item, V oldValue, V newValue) => Validator.MemberChanged (item, oldValue, newValue, member, false);

        public void EntityChanged<T> (T item, string member = null) => Validator.EntityChanged (item, member);

        #endregion

        protected virtual void Instrument () { }

        protected virtual void Dispose (bool disposing) {
            ClearChanges ();
            if (_identityMap != null)
                if (_identityMapOwner)
                    IdentityMap.Dispose ();
                else
                    IdentityMap.Clear ();

            if (disposing) {
                ItemFactory = null;
            }
        }

        public virtual void Dispose () {
            Dispose (true);
        }

        ~Store () {
            Dispose (false);
        }
    }

    public class Store<F> : Store where F : IFactory, new() {
        protected override void Instrument () {
            base.Instrument ();
            if (ItemFactory == null)
                ItemFactory = new F ();
        }
    }

}