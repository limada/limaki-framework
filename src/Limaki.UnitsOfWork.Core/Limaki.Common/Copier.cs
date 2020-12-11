/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2008-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Limaki.Common.Reflections;

namespace Limaki.Common {

    public class Copier {

        public static Func<PropertyInfo, bool> MemberFilter (CopierOptions options, Type attribute = null) {
            attribute ??= typeof(DataMemberAttribute);
            if (options == CopierOptions.DataMember) {
                return p => p != null && p.IsDefined (attribute, true);
            }

            if (options == CopierOptions.ValueTypes) {
                return p => {
                    if (p == null) return false;
                    var type = p.PropertyType;
                    return type.IsPublic && p.CanWrite &&
                           (type.IsEnum || type.IsPrimitive || type.IsValueType || type == typeof(string));
                };
            }

            return p => false;
        }

        public static Action<E, E> CopyAction<E> (CopierOptions options) {
            var delegateType = typeof(Action<E, E>);
            var sourceType = typeof(E);
            var sinkType = typeof(E);

            var sourceExpr = Expression.Variable (sourceType, "source");
            var destExpr = Expression.Variable (sinkType, "sink");

            var memberFilter = MemberFilter (options);

            var paras = new ParameterExpression[] {sourceExpr, destExpr};
            var blockExpressions = new List<Expression> ();
            try {
                foreach (var sourceInfo in sourceType.GetProperties (BindingFlags.Public | BindingFlags.Instance).Where (memberFilter)) {
                    var sourceMember = Expression.Property (sourceExpr, sourceInfo);
                    var destInfo = sinkType.GetProperty (sourceInfo.Name, sourceInfo.PropertyType);
                    var destMember = Expression.Property (destExpr, destInfo);
                    blockExpressions.Add (Expression.Assign (destMember, sourceMember));
                }

                var copyExpression = Expression.Lambda (delegateType, Expression.Block (blockExpressions), paras);
                return (Action<E, E>) copyExpression.Compile ();
            } catch (Exception ex) {
                throw ex;
            }
        }

    }

    public class Copier<T> : Copier {

        public CopierOptions Option { get; protected set; }

        public Func<PropertyInfo, bool> CustomMemberFilter { get; }

        public Copier () {
            this.Attribute = typeof(DataMemberAttribute);
            this.Option = CopierOptions.DataMember;
        }

        public Copier (CopierOptions options)
            : this () {
            this.Option = options;
        }

        public Copier (Func<PropertyInfo, bool> customMemberFilter) {
            CustomMemberFilter = customMemberFilter;
        }

        public Type Attribute { get; protected set; }

        protected virtual Func<PropertyInfo, bool> MemberFilter () => CustomMemberFilter ?? MemberFilter (Option, Attribute);

        protected static readonly Type _thisGenericParameter = typeof(T);

        
        protected static readonly IDictionary<int, Delegate> CopyActionCache = new Dictionary<int, Delegate> ();

        protected static readonly IDictionary<int, MemberReflectionCache> MemberReflectionCache = new Dictionary<int, MemberReflectionCache> ();
        int Key (Type sourceType, Type sinkType) => KeyMaker.GetHashCode (typeof(T), sourceType, sinkType, CustomMemberFilter?.GetHashCode () ?? (int) Option);

        public T Copy (T source, T sink) {
            if (source == null || sink == null) {
                return sink;
            }

            var sourceType = source.GetType ();
            var sinkType = sink.GetType ();

            var key = Key (sourceType, sinkType);

            if (!CopyActionCache.TryGetValue (key, out var copyAction)) {
                var delegateType = typeof(Action<,>).MakeGenericType (sourceType, sinkType);
                var sourceExpr = Expression.Variable (sourceType, "source");
                var destExpr = Expression.Variable (sinkType, "sink");

                var memberFilter = MemberFilter ();
                if (!MemberReflectionCache.TryGetValue (key, out var cache)) {
                    cache = new MemberReflectionCache ();
                    MemberReflectionCache.Add (key, cache);
                    cache.AddType (sinkType, memberFilter);
                }

                var sourceMembers = cache.Members (sourceType, memberFilter);

                var paras = new ParameterExpression[] {sourceExpr, destExpr};
                var blockExpressions = new List<Expression> ();
                try {
                    foreach (var sourceInfo in sourceMembers) {
                        if (cache.ValidMember (sinkType, sourceInfo.PropertyType, sourceInfo.Name)) {
                            var sourceMember = Expression.Property (sourceExpr, sourceInfo);
                            var destInfo = sinkType.GetProperty (sourceInfo.Name, sourceInfo.PropertyType);
                            var destMember = Expression.Property (destExpr, destInfo);
                            blockExpressions.Add (Expression.Assign (destMember, sourceMember));
                        }
                    }

                    var copyExpression = Expression.Lambda (delegateType, Expression.Block (blockExpressions), paras);
                    copyAction = copyExpression.Compile ();
                } catch (Exception ex) {
                    throw ex;
                }

                CopyActionCache.Add (key, copyAction);
            }

            copyAction.DynamicInvoke (new object[] {source, sink});
            // this is slower:
            //delegateType.InvokeMember("Invoke",
            //BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
            //null, copyAction, new object[] { source, sink });

            return sink;
        }

    }

    public enum CopierOptions {
        DataMember,
        ValueTypes,
    }

}