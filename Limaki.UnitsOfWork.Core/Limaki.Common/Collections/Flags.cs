﻿/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Limaki.Common.Collections {

    public class Flags<T> : IEnumerable<T> {

        protected HashSet<T> _flags = new HashSet<T> ();

        protected Flags (HashSet<T> flags) => _flags = flags;

        public Flags () { }

        public Flags (params T[] flags) { Add (flags); }

        public Flags<T> Add (T flag) {
            _flags.Add (flag);
            return this;
        }

        public Flags<T> Add (params T[] flags) => Add ((IEnumerable<T>)flags);

        public Flags<T> Add (IEnumerable<T> flags) {
            foreach (var f in flags)
                Add (f);
            return this;
        }

        public Flags<T> Remove (T flag) {
            _flags.Remove (flag);
            return this;
        }

        public Flags<T> Remove (params T[] flags) => Remove ((IEnumerable<T>)flags);
        public Flags<T> Remove (IEnumerable<T> flags) {
            foreach (var f in flags)
                Remove (f);
            return this;
        }

        public bool HasFlag (T flag) => _flags.Contains (flag);

        public bool HasFlag (Flags<T> other) => _flags.IsSupersetOf (other._flags) || _flags.SetEquals (other._flags);

        public bool HasFlag (IEnumerable<T> other) {
            var result = other?.Any () ?? false;
            if (result)
                foreach (var f in other) result = result && _flags.Contains (f);
            return result;
        }

        public bool HasFlag (params T[] other) => HasFlag ((IEnumerable<T>)other);

        public bool Overlaps (Flags<T> other) => _flags.Overlaps (other._flags);
        public bool IsSubsetOf (Flags<T> other) => _flags.IsSubsetOf (other._flags);

        public override bool Equals (object obj) {
            if (base.Equals (obj))
                return true;
            if (obj is Flags<T> other) {
                return _flags.SetEquals (other._flags);
            }
            if (obj is T flag) {
                return this == flag;
            }
            return false;
        }

        public static bool operator == (Flags<T> flag1, Flags<T> flag2) {
            if (object.ReferenceEquals (flag1, null)) {
                return object.ReferenceEquals (flag2, null);
            }

            return flag1.Equals (flag2);
        }

        public static bool operator == (Flags<T> flags1, T flag2) => flags1._flags.Count == 1 && flags1._flags.Contains (flag2);
        public static bool operator != (Flags<T> flags1, T flag2) => !(flags1 == flag2);

        public static bool operator != (Flags<T> flag1, Flags<T> flag2) => !(flag1 == flag2);

        public static implicit operator Flags<T>(T value) => new Flags<T> (value);

        protected static IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        public override int GetHashCode () {
            if (_flags == null || _flags.Count == 0)
                return 0;
            int h = comparer.GetHashCode (_flags.First ());
            foreach (var f in _flags.Skip (1))
                h = (h << 5) - h + comparer.GetHashCode (f);
            return h;
        }


        static Flags<T> Create (Type type) {
            return (Flags<T>)Activator.CreateInstance (type);
        }

        public override string ToString () => string.Join (",", _flags.Select (NameOf));
        public virtual string ToString (Func<string, string> f) => string.Join (",", _flags.Select (s => f (NameOf (s))));

        public static Flags<T> operator & (Flags<T> c1, Flags<T> c2) {
            var result = Create (c1.GetType ());
            result._flags.UnionWith (c1._flags);
            result._flags.IntersectWith (c2._flags);
            return result;
        }

        public static Flags<T> operator & (Flags<T> c1, T g) {
            var add = c1._flags.Contains (g);
            c1 = Create (c1.GetType ());
            if (add)
                return c1.Add (g);
            return c1;
        }

        public static Flags<T> operator | (Flags<T> c1, Flags<T> c2) {
            return Create (c1.GetType ()).Add (c1._flags).Add (c2._flags);
        }

        public static Flags<T> operator | (Flags<T> c1, T g) => Create (c1.GetType ()).Add (g);

        protected static PropertyInfo[] _flagFields = null;
        protected PropertyInfo[] FlagFields => _flagFields ?? (_flagFields = GetType ().GetProperties (BindingFlags.Static | BindingFlags.Public)
                                                                        .Where (p => p.PropertyType == typeof (T))
                                                                        .ToArray ());

        public string NameOf (T id) => FlagFields.FirstOrDefault (p => p.PropertyType == typeof (T) && p.GetValue (this).Equals (id))
                ?.Name ?? id.ToString ();

        private string DisplayName (Guid id) {
            throw new NotImplementedException ();
            return (FlagFields
                .FirstOrDefault (p => p.PropertyType == typeof (Guid) && ((Guid)p.GetValue (this)) == id)
                ?.GetCustomAttributes (typeof (DisplayAttribute), false)
                .FirstOrDefault () as DisplayAttribute)
                ?.Name;


        }

        public T FlagOf (string name) {
            if (name == null)
                return default;
            var r = FlagFields.FirstOrDefault (p => p.PropertyType == typeof (T) && (p.Name == name))
                ?.GetValue (this) ?? default (T);
            return (T)r;
        }

        public IEnumerator<T> GetEnumerator () => ((IEnumerable<T>)_flags).GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator () => ((IEnumerable<T>)_flags).GetEnumerator ();

        public IEnumerable<T> All () => FlagFields.Select (p => p.GetValue (this)).Cast<T> ();

        public static F WithAll<F> () where F : Flags<T>, new() {
            var f = new F (); f.Add (f.All ()); return f;
        }
    }
}