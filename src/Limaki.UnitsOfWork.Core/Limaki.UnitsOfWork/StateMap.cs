/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Limaki.Common;

namespace Limaki.UnitsOfWork {

    public class StateMap:IDisposable {
        // remark: with Net 4, refactor to:
        // StateMapOf<T>:IStateMapOf { IColl<T> created, updated, removed}}
        // IStateMapOf {IColl<object> created, updated, removed}}
        // StateMap: {Dict<Type,IStateMapOf>


        public virtual IFactory ItemFactory { get; set; }

        protected IDictionary<Type, object> _created = null;
        protected IDictionary<Type, object> _updated = null;
        protected IDictionary<Type, object> _removed = null;

        protected IDictionary<Type, object> created => _created ?? (_created = new Dictionary<Type, object> ());
        protected IDictionary<Type, object> updated => _updated ?? (_updated = new Dictionary<Type, object> ());
        protected IDictionary<Type, object> removed => _removed ?? (_removed = new Dictionary<Type, object> ());

        protected ICollection<T> CreateCollection<T> (IEqualityComparer<T> comparer) => comparer != null ? new HashSet<T> (comparer) : new HashSet<T> ();

        protected ICollection<T> Collection<T>(IDictionary<Type, object> list) {
            ICollection<T> result = null;

            list.TryGetValue (typeof (T), out object o);
            if (o == null) {
                IEqualityComparer<T> comparer = null;
                if (ItemFactory != null)
                    comparer = ItemFactory.Create<IEqualityComparer<T>>();

                result = CreateCollection<T>(comparer);
                list.Add(typeof(T), result);
            } else {
                result = o as ICollection<T>;
            }
            return result;
        }

        public ICollection<T> Created<T> () => Collection<T> (created);
        public ICollection<T> Updated<T> () => Collection<T> (updated);
        public ICollection<T> Removed<T> () => Collection<T> (removed);

        public void AddCreated<T>(T item) {
            if (item != default) {
                Collection<T>(created).Add(item);
            }
        }

        public void Update<T>(T item) {
            if (item != default) {
                Collection<T>(updated).Add(item);
                //Collection<T>(created).Remove(item);
            }
        }

        public void Remove<T>(T item) {
            if (item != default) {
                if (!Created<T>().Contains(item)) {
                    Collection<T>(removed).Add(item);
                }
                Collection<T>(created).Remove(item);
                Collection<T>(updated).Remove(item);
            }
        }

        public int ChangeCount () {
            int result = 0;
            void count (object list) {
                var prop = list.GetType ().GetProperties ().FirstOrDefault (m => m.Name == "Count");
                if (prop != null) {
                    result += (int)prop.GetValue (list, null);
                }
            }
            foreach (var list in created) count (list.Value);
            foreach (var list in updated) count (list.Value);
            foreach (var list in removed) count (list.Value);
            return result;
        }

        public virtual void ClearChanges() {
            created.Clear();
            _created = null;
            updated.Clear();
            _updated = null;
            removed.Clear();
            _removed = null;
        }


        public virtual void Dispose(bool disposing) {
            ClearChanges();
        }

        public virtual void Dispose() {
            Dispose(true);
        }

        ~StateMap() {
            Dispose(false);
        }

 
    }
}