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

using System.Collections.Generic;
using System.Linq;
using System;

namespace Limaki.Common {

    public interface ICollectionWrapper{}

    /// <summary>
    /// this wrapps a collection to be contravariant
    /// that means, that ICollection of T can be used as ICollection of S
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class VarianceCollection<S, T> : ICollection<S>, ICollectionWrapper where T : S {

        public VarianceCollection(ICollection<T> list) {
            this.Collection = list;
        }

        public virtual ICollection<T> Collection { get; protected set; }

        public void Add(S item) {
            if (item is T)
                Collection.Add((T)item);
            else
                throw new ArgumentException();
        }

        public void Clear() {
            Collection.Clear();
        }

        public bool Contains(S item) {
            if (item is T)
                return Collection.Contains((T)item);
            else
                return false;
        }

        public void CopyTo(S[] array, int arrayIndex) {
            var e = Collection.GetEnumerator();
            for (int i = arrayIndex; i < array.Length; i++) {
                if (!e.MoveNext())
                    break;
                array[i] = e.Current;
            }
        }

        public int Count {
            get { return Collection.Count; }
        }

        public bool IsReadOnly {
            get { return Collection.IsReadOnly; }
        }

        public bool Remove(S item) {
            if (item is T)
                return Collection.Remove((T)item);
            else
                return false;
        }


        public IEnumerator<S> GetEnumerator() {
            return Collection.Cast<S>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }


    }

    public class VarianceList<S, T> : VarianceCollection<S, T>, IList<S>, ICollectionWrapper where T : S {
        public VarianceList(ICollection<T> list):this(list as IList<T>) {}

        public VarianceList(IList<T> list):base(list) {
            if (list == null)
                throw new ArgumentException();
        }

        public virtual IList<T> List { get { return this.Collection as IList<T>;}  }

        public int IndexOf(S item) {
             if (item is T) {
                 return List.IndexOf((T)item);
             }
            return -1;
        }

        public void Insert(int index, S item) {
            if (item is T) {
                List.Insert(index, (T)item);
            } else {
                throw new ArgumentException();
            } 
        }

        public void RemoveAt(int index) {
            List.RemoveAt(index);
        }

        public S this[int index] {
            get {
                return List[index];
            }
            set {
                if (value is T) {
                    List[index] = (T)value;
                } else {
                    throw new ArgumentException();
                } 
            }
        }

       
    }
}