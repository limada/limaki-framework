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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Limaki.Common;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {

    public class IdentityMap : IDisposable {

        public virtual IFactory ItemFactory { get; set; }

        IDictionary<Type, object> _map = null;
        protected IDictionary<Type, object> Map => _map ?? (_map = new Dictionary<Type, object> ());

        public string Info () {
            var count = Map.Values.OfType<IList> ().Sum (m => m.Count);
            return $"{GetType ().Name} : {Map.Count ()} types with {count} items";
        }

        protected IIdentityList<T> TryGetCreate<T> (IDictionary<Type, object> map) {
            IIdentityList<T> result = null;

            if (!map.TryGetValue (typeof (T), out object o)) {
                result = ItemFactory.Create<IIdentityList<T>> ();
                if (result != null)
                    map.Add (typeof (T), result);
            } else {
                result = o as IIdentityList<T>;
            }
            return result;
        }

        public IEnumerable<T> Stored<T> () => TryGetCreate<T> (Map);

        public void Clear<T> () => Map.Remove (typeof (T));

        public bool Add<T> (T item) {
            if (item == null) return false;
            var list = TryGetCreate<T> (Map);
            return list != null && list.Add (item);
        }

        public void AddRange<T> (IEnumerable<T> items) {
            var result = false;
            if (items != null) {
                var list = TryGetCreate<T> (Map);
                if (list != null) {
                    foreach (var item in items) {
                        result = list.Add (item);
                    }
                }
            }
        }

        public bool Refresh<T> (T item) {
            var result = false;
            if (item != null) {
                var list = TryGetCreate<T> (Map);
                if (list != null) {
                    result = list.Refresh (item);
                }
            }
            return result;
        }

        public bool RefreshRange<T> (IEnumerable<T> items) {
            var result = false;
            if (items != null) {
                var list = TryGetCreate<T> (Map);
                if (list != null) {
                    foreach (var item in items) {
                        result = list.Refresh (item);
                    }
                }
            }
            return result;
        }

        public IEnumerable<T> Unique<T> (IEnumerable<T> items) {
            if (items != null) {
                var list = TryGetCreate<T> (Map);
                if (list != null) {
                    var stack = new Stack<T> ();
                    foreach (var item in items) {
                        stack.Push (list.Unique (item));
                    }
                    return stack;
                }
            }
            return new T[0];
        }

        public T Unique<T> (T item) {
            var result = item;
            if (item != null) {
                var list = TryGetCreate<T> (Map);
                if (list != null) {
                    result = list.Unique (item);
                }
            }
            return result;
        }

        public T Item<T, TKey> (TKey key) {
            var result = default (T);
            if (TryGetCreate<T> (Map) is IdentityList<TKey, T> list && list.Contains (key)) {
                return list[key];
            }
            return result;
        }

        public T Item<T> (Func<T, bool> predicate) {
            var list = TryGetCreate<T> (Map);
            if (list != null) {
                var lookup = list.FirstOrDefault (predicate);
                return lookup;
            }
            return default (T);
        }

        public bool Contains<T, TKey> (TKey key) => TryGetCreate<T> (Map) is IdentityList<TKey, T> list && list.Contains (key);

        public bool Contains<T> (Func<T, bool> predicate) {
            var list = TryGetCreate<T> (Map);
            return list.Any (predicate);
        }

        public void Remove<T> (T item) {
            if (item != null) {
                var list = TryGetCreate<T> (Map);
                if (list != null)
                    list.Remove (item);
            }
        }

        public void Remove<T> (IEnumerable<T> items) {
            if (items == null) return;
            foreach (var item in items) Remove (item); 
        }

        public virtual void Clear () {
            if (_map != null) {
                //foreach (var item in _map)
                //    (item.Value as IIdentityList).Clear();
                _map.Clear ();
            }
            _map = null;
        }

        public virtual void Dispose (bool disposing) {
            Clear ();
            if (disposing) {
                ItemFactory = null;
            }
        }

        public virtual void Dispose () {
            Dispose (true);
        }

        public IdentityMap Clone () {
            var result = new IdentityMap ();
            result.ItemFactory = this.ItemFactory;
            foreach (var list in Map)
                result.Map[list.Key] = list.Value;
            return result;
        }

        ~IdentityMap () {
            Dispose (false);
        }


    }
}