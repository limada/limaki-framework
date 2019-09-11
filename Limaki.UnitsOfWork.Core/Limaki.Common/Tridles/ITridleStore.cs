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
using System.Linq.Expressions;
using System.Reflection;

namespace Limaki.Common.Tridles {

    public interface ITridleStore<K> {

        ITridle<K, string> DynType (K dynId);

        #region bindings to external store

        Func<K, K, object, ITridle<K>> Resolve { get; set; }

        Func<K, K, ITridle<K, string>> TypeDefResolve { get; set; }
        Func<ITridle<K>, bool> Upsert { get; set; }

        Func<K, K, ITridle<K>> DynValueResolve { get; set; }

        #endregion
        
        Tridlet<K> Tridlet { get; }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="E">entity</typeparam>
    /// <typeparam name="K">key or id</typeparam>
    public interface ITridleStore<E, K> : ITridleStore<K>, IDynValueStore<E> {

        /// <summary>
        /// binding to get Id of T
        /// </summary>
        Func<E, K> IdOfT { get; }
        
        #region typeof(T) metadata
        
        K TypeId { get; }
        ITridle<K, string> TypeDef { get; }

        #endregion

        #region dynanmic entity methods

        /// <summary>
        /// tridle of a dynamic property's type
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        ITridle<K, string> DynType<V> (string name);
        
        ITridle<K, string> DynDef<V> (string name);

        ITridle<K, V> SetDynTridle<V> (E entity, string name, V value);
        ITridle<K, V> GetDynTridle<V> (E entity, string name);

        IEnumerable<ITridle<K>> DynTridles (E entity);

        #endregion

        #region type member methods

        ITridle<K, string> MemberDef<V> (Expression<Func<E, V>> member);
        ITridle<K, string> MemberDef (PropertyInfo member);

        /// <summary>
        /// tridles that holds typeof(T) name and properties
        /// </summary>
        /// <returns></returns>
        IEnumerable<ITridle<K>> TypeDefs ();

        /// <summary>
        /// tridles that holds the values of the properties of entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        IEnumerable<ITridle<K>> MemberTridles (E entity);

        #endregion

    }
}