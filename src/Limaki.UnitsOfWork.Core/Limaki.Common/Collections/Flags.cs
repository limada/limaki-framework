/*
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
using Limaki.UnitsOfWork;

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
            if (flags != default)
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
            if (flags != default)
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

        /// <summary>
        /// Determines whether the current Flags object and other share common elements.
        /// </summary>
        public bool Overlaps (Flags<T> other) => other != default && _flags.Overlaps (other._flags);
        /// <summary>
        /// Determines whether the current Flags object and other share common elements.
        /// </summary>
        public bool Overlaps (params T[] other) => _flags.Overlaps (other);
        /// <summary>
        /// Determines whether the current Flags object and other share common elements.
        /// </summary>
        public bool Overlaps (IEnumerable<T> other) => other != default && _flags.Overlaps (other);

        /// <summary>
        /// Determines whether the current Flags object is a subset of other
        /// </summary>
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
            if (ReferenceEquals (flag1, null)) {
                return ReferenceEquals (flag2, null);
            }

            return flag1.Equals (flag2);
        }

        static bool _isClass = typeof (T).IsClass;
        static bool IsDefault (T flag) => _isClass ? flag == null : flag.Equals (default (T));
        public static bool operator == (Flags<T> flags1, T flag2) => ReferenceEquals (flags1, null) ?
                                                                     IsDefault (flag2) :
                                                                     flags1._flags.Count == 1 && flags1._flags.Contains (flag2);

        public static bool operator != (Flags<T> flags1, T flag2) => !(flags1 == flag2);

        public static bool operator != (Flags<T> flag1, Flags<T> flag2) => !(flag1 == flag2);

        public static implicit operator Flags<T>(T value) => new Flags<T> (value);

        public static implicit operator Flags<T>(T[] value) => new Flags<T> (value);

        public static Flags<T> operator & (Flags<T> c1, Flags<T> c2) => Intersect (c1, Create (c1.GetType ()), c2);

        public static Flags<T> operator & (Flags<T> c1, IEnumerable<T> c2) => Intersect (c1, Create (c1.GetType ()), c2.ToArray ());

        public static Flags<T> operator & (Flags<T> c1, T g) {
            var add = c1._flags.Contains (g);
            c1 = Create (c1.GetType ());
            if (add)
                return c1.Add (g);
            return c1;
        }

        public static Flags<T> operator | (Flags<T> c1, Flags<T> c2) => Create (c1.GetType ()).Add (c1._flags).Add (c2._flags);

        public static Flags<T> operator | (Flags<T> c1, T g) => Create (c1.GetType ()).Add (g);

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

        // WARNING! do not make ist static; All() fails then, as first Flags<T>-class takes _flagFields over
        // needs a more sophisticated static cache; eg: Dictionary<Type,PropertyInfo []> _cache and call with _cache.TryGet(this.gettype)
        protected PropertyInfo[] _flagFields = null;
        protected PropertyInfo[] FlagFields => _flagFields ?? (_flagFields = GetType ().GetProperties (BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                                        .Where (p => p.PropertyType == typeof (T))
                                                                        .ToArray ());

        public virtual string NameOf (T id) => TryNameOf (id, out string name) ? name : id.ToString ();

        public virtual bool TryNameOf (T id, out string name) {
            var property = FlagFields.FirstOrDefault (p => p.PropertyType == typeof (T) && p.GetValue (null).Equals (id));
            name = property?.Name;
            return property != default;
        }

        internal void AddNames<F>(F other) where F : Flags<T>{
            var fields = FlagFields.ToList();
            foreach (var field in other.FlagFields) {
                fields.Add(field);
            }
            _flagFields = fields.Distinct().ToArray();
        }

        public Flags<T> WithNames<F>() where F : Flags<T>, new() {
            AddNames(new F());
            return this;

        }

        private string DisplayName (Guid id) {
            throw new NotImplementedException ();
            return (FlagFields
                .FirstOrDefault (p => p.PropertyType == typeof (Guid) && ((Guid)p.GetValue (this)) == id)
                ?.GetCustomAttributes (typeof (DisplayAttribute), false)
                .FirstOrDefault () as DisplayAttribute)
                ?.Name;


        }

        public virtual T FlagOf (Type type) {
            if (type == null)
                return default;
            var r = FlagFields.FirstOrDefault (p => p.PropertyType == typeof (T)
                && (p.GetCustomAttributes<TypeGuidAttribute> ().Any (t => t.Type == type)))
                ?.GetValue (this) ?? default (T);
            return (T)r;
        }

        public virtual T FlagOf (string name) {
            if (name == null)
                return default;
            var r = FlagFields.FirstOrDefault (p => p.PropertyType == typeof (T) && (p.Name == name))
                ?.GetValue (this) ?? default (T);
            return (T)r;
        }

        public virtual IEnumerator<T> GetEnumerator () => ((IEnumerable<T>)_flags).GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator () => this.GetEnumerator ();

        public virtual IEnumerable<T> All () => FlagFields.Select (p => p.GetValue (this)).Cast<T> ();


        public static F WithAll<F> () where F : Flags<T>, new() {
            var f = new F (); f.Add (f.All ()); return f;
        }

        public static F With<F> (params F[] others) where F : Flags<T>, new() {
            var f = new F ();
            foreach (var other in others.Where (o => o != default))
                f.Add (other);
            return f;
        }

        internal static F Intersect<F> (F source, F dest, params IEnumerable<T>[] others) where F : Flags<T> {
            if (others == default || source == default || dest == default)
                return dest;
            dest._flags.UnionWith (source._flags);
            foreach (var other in others) {
                if (other != default)
                    dest._flags.IntersectWith (other);
                else {
                    dest._flags.Clear ();
                    break;
                }
            }

            return dest;

        }


    }

    public static class FlagsExtensions {

        public static F _nextversionof_With<F,T> (this F it, params IEnumerable<T>[] others) where F : Flags<T>, new() {
            var f = new F ();
            f.Add (it);
            foreach (var other in others.Where (o => o != default))
                f.Add (other);
            return f;
        }

        public static F With<F, T> (this F it, params T[] flags) where F : Flags<T>, new() {
            var f = new F (); f.AddNames(it); f.Add (it); f.Add (flags); return f;
        }

        public static F WithOut<F, T> (this F it, params T[] flags) where F : Flags<T>, new() {
            var f = new F (); f.Add (it); f.Remove (flags); return f;
        }

        public static F IntersectWith<F,T> (this F c1, params IEnumerable<T>[] others) where F : Flags<T>, new() => Flags<T>.Intersect (c1, new F (), others);

    }
}
