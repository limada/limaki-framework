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

namespace Limaki.Common.Collections {

    public class IdentityMap : IDisposable {
        public virtual IFactory ItemFactory { get; set; }

        protected IDictionary<Type, object> _map = null;
        protected IDictionary<Type, object> Map {
            get { return _map ?? (_map = new Dictionary<Type, object>()); }
        }

        
        protected IIdentityList<T> TryGetCreate<T>(IDictionary<Type, object> map) {
            IIdentityList<T> result = null;
            object o = null;

            map.TryGetValue(typeof(T), out o);
            if (o == null) {
                result = ItemFactory.Create<IIdentityList<T>>();
                if(result!=null)
                    map.Add(typeof(T), result);
            } else {
                result = o as IIdentityList<T>;
            }
            return result;
        }

        public IEnumerable<T> Stored<T>() {
            return TryGetCreate<T>(Map);
        }

        public void Clear<T>() {
            Map.Remove(typeof(T));
        }

        public bool Add<T>(T item) {
            var result = false;
            if (item != null) {
                var list = TryGetCreate<T>(Map);
                if (list != null) {
                    result = list.Add(item);
                }
            }
            return result;
        }

        public void AddRange<T>(IEnumerable<T> items) {
            var result = false;
            if (items != null) {
                var list = TryGetCreate<T>(Map);
                if (list != null) {
                    foreach (var item in items) {
                        result = list.Add(item);
                    }
                }
            }
        }

        public bool Refresh<T>(T item) {
            var result = false;
            if (item != null) {
                var list = TryGetCreate<T>(Map);
                if (list != null) {
                    result = list.Refresh(item);
                }
            }
            return result;
        }


        public IEnumerable<T> Unique<T>(IEnumerable<T> items) {
            if (items != null) {
                var list = TryGetCreate<T>(Map);
                if (list != null) {
                    var stack = new Stack<T>();
                    foreach (var item in items) {
                        stack.Push(list.Unique(item));
                    }
                    return stack;
                }
            }
            return new T[0];
        }

        public T Unique<T>(T item) {
            var result = item;
            if (item != null) {
                var list = TryGetCreate<T>(Map);
                if (list != null) {
                    result = list.Unique(item);
                }
            }
            return result;
        }

        public T Item<T, TKey>(TKey key) {
            var result = default(T);
            var list = TryGetCreate<T>(Map) as IdentityList<TKey, T>;
            if (list != null && list.Contains(key)) {
                return list[key];
            }
            return result;
        }

        public T Item<T>(Func<T, bool> predicate) {
            var list = TryGetCreate<T>(Map);
            if(list!=null) {
                var lookup = list.FirstOrDefault(predicate);
                return lookup;
            }
            return default(T);
        }


        public bool Contains<T, TKey>(TKey key) {
            var list = TryGetCreate<T>(Map) as IdentityList<TKey, T>;
            return list != null && list.Contains(key);
        }

        public bool Contains<T>(Func<T, bool> predicate) {
            var list = TryGetCreate<T>(Map);
            return list.Any(predicate);
        }

        public void Remove<T>(T item) {
            if (item != null) {
                var list = TryGetCreate<T>(Map);
                if (list != null)
                    list.Remove(item);
            }
        }

        //public virtual DelegatePool IdentityDelegates { get; set; }
        //public IIdentityList<TItem> CreateIdentityList<TItem, TKey>() {
        //    var keyFunc = IdentityDelegates.Get<TItem, Func<TItem, TKey>>();
        //    if(keyFunc ==null) {
        //        throw new ArgumentException (typeof (TItem).Name + " needs an IdentityFunc");
        //    }

        //    return new DelegatingKeyedCollection<TKey, TItem>(keyFunc);
        //}

        public virtual void Clear() {
            if (_map != null) {
                //foreach (var item in _map)
                //    (item.Value as IIdentityList).Clear();
                _map.Clear();
            }
            _map = null;
        }

        public virtual void Dispose(bool disposing) {
            Clear();
            if (disposing) {
                ItemFactory = null;
            }
        }

        public virtual void Dispose() {
            Dispose(true);
        }

        ~IdentityMap() {
            Dispose(false);
        }


    }
}