/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2010 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Limaki.Common;
using Limaki.Common.Linqish;

namespace Limaki.Common.Collections {

    public interface IIdentityList {

        void Clear ();

    }

    public interface IIdentityList<T> : IList<T>, IIdentityList {

        TKey GetKeyForItem<TKey> (T item);

        object GetKeyForItem (T item);

        T Unique (T item);

        new bool Add (T item);

        bool Refresh (T item);

    }

    public interface IIdentityList<TKey, TItem> : IIdentityList<TItem> {

        Func<TItem, TKey> KeyFunc { get; }

        TItem GetItem (TKey key);

    }

    public class IdentityList<TKey, TItem> : KeyedCollection<TKey, TItem>, IIdentityList<TKey, TItem> {

        public IdentityList (Func<TItem, TKey> keyFunc) {
            this.KeyFunc = keyFunc;
        }

        public IdentityList (Func<TItem, TKey> keyFunc, IEnumerable<TItem> list) : this (keyFunc) {
            AddRange (list);
        }

        public Func<TItem, TKey> KeyFunc { get; set; }

        protected override TKey GetKeyForItem (TItem item) {
            return KeyFunc (item);
        }

        public virtual TItem GetItem (TKey key) {
            if (Contains (key))
                return this[key];

            return default;
        }

        public virtual TItem Unique (TItem item) {
            var result = item;
            var key = KeyFunc (item);

            if (this.Contains (key))
                result = this[key];
            else
                base.Add (item);

            return result;
        }

        public virtual new bool Add (TItem item) {
            var result = !this.Contains (KeyFunc (item));

            if (result) {
                base.Add (item);
            }

            return result;
        }

        void ICollection<TItem>.Add (TItem item) {
            this.Add (item);
        }

        public virtual void AddRange (IEnumerable<TItem> items) {
            if (items == null)
                return;

            items.ForEach (i => this.Add (i));
        }

        public virtual bool Refresh (TItem item) {
            if (this.Contains (KeyFunc (item))) {
                var stored = this[KeyFunc (item)];
                new Copier<TItem> ().Copy (item, stored);

                return true;
            } else {
                base.Add (item);

                return false;
            }
        }

        public virtual new bool Remove (TItem item) {
            var key = GetKeyForItem (item);

            return base.Remove (key);
        }

        TKey1 IIdentityList<TItem>.GetKeyForItem<TKey1> (TItem item) {
            if (typeof(TKey).Equals (typeof(TKey1)))
                return (TKey1) (object) this.GetKeyForItem (item);

            throw new ArgumentException (typeof(TKey1).Name + " must be" + typeof(TKey));
        }

        object IIdentityList<TItem>.GetKeyForItem (TItem item) {
            return this.GetKeyForItem (item);
        }

        bool ICollection<TItem>.Contains (TItem item) {
            return this.Contains (KeyFunc (item));
        }

        protected override void ClearItems () {
            base.ClearItems ();

            if (this.Dictionary != null)
                this.Dictionary.Clear ();
        }

        protected virtual void Dispose (bool disposing) {
            ClearItems ();
        }

        public virtual void Dispose () {
            Dispose (true);
        }

        ~IdentityList () {
            Dispose (false);
        }

    }

}