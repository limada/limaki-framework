/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2011 Lytico
 *
 * http://www.limada.org
 * 
 */


using System.Collections.Generic;
using System;

namespace Limaki.Common.Collections {

    public class FuncComparer<T> : Comparer<T> {
        Func<T, T, int> Comparer { get; set; }
        public FuncComparer (Func<T, T, int> comparer) {
            this.Comparer = comparer;
        }
        public override int Compare (T x, T y) {
            return Comparer (x, y);
        }
    }

    public class EmptyCollection<T> : ICollection<T> {

        #region ICollection<T> Member

        public void Add (T item) { }

        public void Clear () { }

        public bool Contains (T item) => false;

        public void CopyTo (T[] array, int arrayIndex) { }

        public int Count => 0;

        public bool IsReadOnly => true;

        public bool Remove (T item) => false;

        #endregion

        #region IEnumerable<T> Member

        public IEnumerator<T> GetEnumerator () {
            yield break;
        }

        #endregion

        #region IEnumerable Member

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () => GetEnumerator ();

        #endregion
    }

    public struct EmptyEnumerable<T> : IEnumerable<T> {
        #region IEnumerable<T> Member

        public IEnumerator<T> GetEnumerator () {
            yield break;
        }

        #endregion

        #region IEnumerable Member

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () => GetEnumerator ();

        #endregion
    }

    public static class CollectionExtensions { 

        public static void AddRange<T>(this ICollection<T> it, IEnumerable<T> other) {

            foreach (var item in other)
                it.Add (item);

        }
    }
}