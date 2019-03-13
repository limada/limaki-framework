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
using System.Linq.Expressions;

namespace Limaki.Common.Tridles {

    public class Tridlet<K> {

        public Tridlet () {
            MetaId = new MetaId<K> ();
        }
        /// <summary>
        /// func to create a unique id
        /// </summary>
        public Func<K> CreateId { get; set; }

        public MetaId<K> MetaId { get; protected set; }

        public virtual ITridle<K, V> Create<V> (K key, K member, V value) => new Tridle<K, V> { Id = CreateId (), Key = key, Member = member, Value = value };

        public virtual ITridle<K, V> Create<V> (K id, K key, K member, V value) => new Tridle<K, V> { Id = id, Key = key, Member = member, Value = value };

        public ITridle<K> Create (K id, K key, K member, Type valueType, object value) {
            // TODO: this is slow; cache it
            Expression<Action> lambda = () => Create<object> (id, key, member, null);
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (valueType);

            return (ITridle<K>) method.Invoke (this, new object[] { id, key, member, value });
        }

        public ITridle<K> Create (K key, K member, Type valueType, object value) {
            // TODO: this is slow; cache it
            Expression<Action> lambda = () => Create<object> (key, member, null);
            var method = (lambda.Body as MethodCallExpression).Method
                .GetGenericMethodDefinition ().MakeGenericMethod (valueType);

            return (ITridle<K>) method.Invoke (this, new object[] { key, member, value });
        }
    }
}