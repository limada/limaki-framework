/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2011 Lytico
 *
* http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Limaki.Common.Linqish {

    public static class EnumerableExtensions {

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action) {
            if (items == default)
                return;
            foreach (var item in items)
                action(item);

        }

        public static IEnumerable<T> For<T>(this IEnumerable<T> items, Action<T,int> action) {
            if (items == default)
                yield break;
            var i = 0;
            foreach (var item in items) {
                action(item, i++);
                yield return item;
            }
        }

        public static IEnumerable<T> For<T>(this IEnumerable<T> items, Func<T, int,T> func)
        {
            if (items == default)
                yield break;
            var i = 0;
            foreach (var item in items) {
                yield return func(item, i++);
            }
        }

        public static IEnumerable<T> Yield<T> (this IEnumerable<T> items) {
            if (items == default)
                yield break;
            foreach (var item in items)
                yield return item;
        }

        public static IEnumerable<T> OnEach<T> (this IEnumerable<T> items, Func<T, T> func)
        {
            if (items == default)
                yield break;

            foreach (var item in items)
                yield return func(item);
        }

        public static IEnumerable<T> OnEach<T> (this IEnumerable<T> items, Action<T> action) {
            if (items == default)
                yield break;
            foreach (var item in items) {
                action (item);
                yield return item;
            }
        }

    }

    public static class Repeat { 
        
        public static IEnumerable<T> For<T> (this Func<T> it, int count) {
            for (var i = 0; i < count; i++) yield return it ();
        }

        public static IEnumerable<T> For<T>(this Func<int,T> it, int count)
        {
            for (var i = 0; i < count; i++) yield return it(i);
        }

        public static void Action (Action it, int count) {
            for (var i = 0; i < count; i++) { 
                it (); 
            }
        }

        public static void Action(Action<int> it, int count)
        {
            for (var i = 0; i < count; i++) {
                it(i);
            }
        }

    }
}