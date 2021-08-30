/*
 * Tridles 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2015 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common.Linqish;

namespace Limaki.Common.Tridles {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="E">entity</typeparam>
    /// <typeparam name="K">key</typeparam>
    public class TridleStore<E, K> : ITridleStore<E, K> {

        public TridleStore (Tridlet<K> tridlet, Func<E, K> idOfT) {
            this.Tridlet = tridlet;
            this.IdOfT = idOfT;
        }

        public Func<E, K> IdOfT { get; protected set; }

        private ITridle<K, string> _typeDef = null;
        public ITridle<K, string> TypeDef {
            get { return _typeDef ?? (_typeDef = TryGetCreate (_typeDefs, Tridlet.MetaId.Type, Tridlet.MetaId.TypeName, typeof (E).FullName)); }
        }

        private K _typeId = default (K);
        public K TypeId {
            get {
                if (_typeId.Equals (default (K))) {
                    _typeId = TypeDef.Id;
                }
                return _typeId;
            }
        }

        public Func<K, K, object, ITridle<K>> Resolve { get; set; }

        public Func<K, K, ITridle<K>> DynValueResolve { get; set; }

        public Func<K, K, ITridle<K, string>> TypeDefResolve { get; set; }

        public Func<ITridle<K>, bool> Upsert { get; set; }

        public Tridlet<K> Tridlet { get; protected set; }

        private IDictionary<K, ITridle<K, string>> _typeDefs = new Dictionary<K, ITridle<K, string>> ();
        private IDictionary<K, ITridle<K, string>> _memberDefs = new Dictionary<K, ITridle<K, string>> ();
        private IDictionary<K, ITridle<K, string>> _dynDefs = new Dictionary<K, ITridle<K, string>> ();
        private IDictionary<K, ITridle<K, string>> _dynTypes = new Dictionary<K, ITridle<K, string>> ();

        protected ITridle<K, string> TryGetCreate (IDictionary<K, ITridle<K, string>> defs, K key, K member, string value) {

            var c = defs.Values.Where (d => d.Key.Equals (key) && d.Value.Equals (value) && d.Member.Equals (member)).FirstOrDefault ();
            if (c == null) {
                if (Resolve != null) {
                    var r = Resolve (key, member, value);
                    c = r as ITridle<K, string>;
                    if (r != null && c == null)
                        throw new ArgumentException (string.Format ("Resolve error: {0} should be of type {1}", r, typeof (ITridle<K, string>)));
                }

                if (c == null) {
                    c = Tridlet.Create (key, member, value);
                    Upsert?.Invoke (c);
                }

                defs.Add (c.Id, c);
            }
            return c;
        }

        public ITridle<K, string> MemberDef<V> (Expression<Func<E, V>> member) {
            var memberEx = member as MemberExpression;
            CheckMemberType (memberEx.Member);
            return TryGetCreate (_memberDefs, TypeId, Tridlet.MetaId.TypeMember, ExpressionUtils.Nameof (member));
        }

        public ITridle<K, string> MemberDef (PropertyInfo member) {
            CheckMemberType (member);
            return TryGetCreate (_memberDefs, TypeId, Tridlet.MetaId.TypeMember, member.Name);
        }

        protected void CheckMemberType (MemberInfo memberInfo) {
            var prop = memberInfo as PropertyInfo;
            if (prop == null)
                throw new ArgumentException ("Only properties are allowed");
            CheckType (prop.PropertyType);
        }

        protected void CheckType (Type type) {
            if (!memberFilter (type)) {
                throw new ArgumentException ("Only primitive types and strings are allowed");
            }
        }

        private Func<Type, bool> memberFilter = (p => p.IsPrimitive || p == typeof (string));

        public ITridle<K, string> DynType (K dynId) {
            ITridle<K, string> r = null;
            if (_dynTypes.TryGetValue (dynId, out r))
                return r;
            if (TypeDefResolve != null) {
                r = this.TypeDefResolve (dynId, Tridlet.MetaId.MemberType);
                if (r != null)
                    _dynTypes.Add (dynId, r);
            }
            return r;
        }

        public ITridle<K, string> DynDef<V> (string name) {
            CheckType (typeof (V));
            return TryGetCreate (_dynDefs, TypeId, Tridlet.MetaId.Member, name);
        }

        /// <summary>
        /// tridle of a dynamic property's type
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public ITridle<K, string> DynType<V> (string name) {
            CheckType (typeof (V));
            var dynId = TryGetCreate (_dynDefs, TypeId, Tridlet.MetaId.Member, name);
            ITridle<K, string> c = null;
            var dynTypeName = typeof (V).FullName;
            if (!_dynTypes.TryGetValue (dynId.Id, out c)) {
                c = TryGetCreate (_dynTypes, dynId.Id, Tridlet.MetaId.MemberType, dynTypeName);
            } else {
                if (Tridlet.MetaId.MemberType.Equals (c.Member) && c.Value != dynTypeName)
                    throw new ArgumentException (string.Format ("class {0} has already a dynamic property {1} with type {2}. You tried do add a property of type {3}", typeof (E).Name, name, dynTypeName, c.Member));
            }
            return c;
        }

        /// <summary>
        /// tridles that holds typeof(T) name and properties
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITridle<K>> TypeDefs () {

            yield return TypeDef;

            var type = typeof (E);
            foreach (var p in type.GetProperties (BindingFlags.Public | BindingFlags.Instance).Where (p => memberFilter (p.PropertyType))) {
                yield return MemberDef (p);
            }
        }

        /// <summary>
        /// tridles that holds the values of the properties of entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<ITridle<K>> MemberTridles (E entity) {

            var type = typeof (E);
            var key = IdOfT (entity);
            foreach (var p in type.GetProperties (BindingFlags.Public | BindingFlags.Instance).Where (p => memberFilter (p.PropertyType))) {
                var def = MemberDef (p);
                yield return Tridlet.Create (key, def.Id, p.PropertyType, p.GetValue (entity));

            }
        }

        public ITridle<K, V> SetDynTridle<V> (E entity, string name, V value) {
            var dt = DynType<V> (name);
            var tridle = Tridlet.Create<V> (IdOfT (entity), dt.Key, value);
            if (Upsert != null)
                Upsert (tridle);
            return tridle;
        }

        public ITridle<K, V> GetDynTridle<V> (E entity, string name) {
            if (DynValueResolve == null)
                return null;
            var dt = DynType<V> (name);
            var tridle = DynValueResolve (IdOfT (entity), dt.Key);
            var r = tridle as Tridle<K, V>;
            if (tridle != null && r == null)
                throw new ArgumentException ("wrong type of dynamic property");
            return r;
        }

        public void SetDynValue<V> (E entity, string name, V value) {
            SetDynTridle (entity, name, value);
        }

        public V GetDynValue<V> (E entity, string name) {
            var tridle = GetDynTridle<V> (entity, name);
            if (tridle != null)
                return tridle.Value;
            return default (V);
        }

        public IEnumerable<ITridle<K>> DynTridles (E entity) {
            if (DynValueResolve == null)
                yield break;
            foreach (var dt in _dynDefs.Values) {
                var tridle = DynValueResolve (IdOfT (entity), dt.Id);
                if (tridle != null)
                    yield return tridle;
            }
        }
    }
}