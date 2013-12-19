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

namespace Limaki.Common.UnitsOfWork {

    public class StateMap:IDisposable {
        // remark: with Net 4, refactor to:
        // StateMapOf<T>:IStateMapOf { IColl<T> created, updated, removed}}
        // IStateMapOf {IColl<object> created, updated, removed}}
        // StateMap: {Dict<Type,IStateMapOf>


        public virtual IFactory ItemFactory { get; set; }

        protected IDictionary<Type, object> _created = null;
        protected IDictionary<Type, object> _updated = null;
        protected IDictionary<Type, object> _removed = null;

        protected IDictionary<Type, object> created {
            get { return _created ?? (_created = new Dictionary<Type, object>()); }
        }
        protected IDictionary<Type, object> updated {
            get { return _updated ?? (_updated = new Dictionary<Type, object>()); }
        }
        protected IDictionary<Type, object> removed {
            get { return _removed ?? (_removed = new Dictionary<Type, object>()); }
        }

        protected ICollection<T> CreateCollection<T>(IEqualityComparer<T> comparer) {
            if (comparer != null)
                return new HashSet<T>(comparer);
            else
                return new HashSet<T>();
        }

        protected ICollection<T> Collection<T>(IDictionary<Type, object> list) {
            ICollection<T> result = null;
            object o = null;

            list.TryGetValue(typeof(T), out o);
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

        public ICollection<T> Created<T>() {
            return Collection<T>(created);
        }
        public ICollection<T> Updated<T>() {
            return Collection<T>(updated);
        }

        public ICollection<T> Removed<T>() {
            return Collection<T>(removed);
        }

        public void AddCreated<T>(T item) {
            if (item != null) {
                Collection<T>(created).Add(item);
            }
        }

        public void Update<T>(T item) {
            if (item != null) {
                Collection<T>(updated).Add(item);
                //Collection<T>(created).Remove(item);
            }
        }

        public void Remove<T>(T item) {
            if (item != null) {
                if (!Created<T>().Contains(item)) {
                    Collection<T>(removed).Add(item);
                }
                Collection<T>(created).Remove(item);
                Collection<T>(updated).Remove(item);
            }
        }

        public int ChangeCount() {
            int result = 0;
            Action<object> count = list => {
                var prop = list.GetType().GetProperties().Where(m => m.Name == "Count").FirstOrDefault();
                if (prop != null) {
                    result += (int)prop.GetValue(list, null);
                }
            };
            foreach (var list in created) count(list.Value);
            foreach (var list in updated) count(list.Value);
            foreach (var list in removed) {
                var prop = list.Value.GetType().GetProperties().Where(m => m.Name == "Count").FirstOrDefault();
                if (prop != null) {
                    result += (int)prop.GetValue(list.Value, null);
                }
            }
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