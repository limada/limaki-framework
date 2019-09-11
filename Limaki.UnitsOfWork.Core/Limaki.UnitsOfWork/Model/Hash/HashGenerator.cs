/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2019 Lytico
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
using System.Security.Cryptography;
using System.Text;
using Limaki.Common;
using Limaki.Common.Linqish;
using Limaki.Common.Reflections;

namespace Limaki.UnitsOfWork.Model.Hash {

    public class HashGenerator {

        static HashAlgorithm Algo = new MD5CryptoServiceProvider (); //seems to be the fastest

        public static byte [] Hash (byte [] data) {
            byte [] result = Algo.ComputeHash (data);

            return result;
        }

        public static byte [] Hashes (params byte [] [] bytes) {
            //var md5 = MD5.Create();
            int offset = 0;
            var len = 0;
            foreach (var bb in bytes) {
                len += bb.Length;
            }
            var buff = new byte [len];

            foreach (var bb in bytes) {
                for (var i = 0; i < bb.Length - 1; i++) {
                    buff [i + offset] = bb [i];
                }
                offset += bb.Length;
            }

            return Algo.ComputeHash (buff);
        }

        public static string HashString (params byte [] [] bytes) => ToBase64 (Hashes (bytes));

        public static string ToBase64 (byte [] data) => Convert.ToBase64String (data);

        public static byte [] Hash<T> (T data) {
            if (data is string s) {
                return Hash (Encoding.Unicode.GetBytes (s));
            }
            if (data is int i) {
                return Hash (BitConverter.GetBytes (i));
            }
            if (data is long l) {
                return Hash (BitConverter.GetBytes (l));
            }
            if (data is DateTime d) {
                return Hash (BitConverter.GetBytes (d.Ticks));
            }
            if (data is bool b) {
                return Hash (BitConverter.GetBytes (b));
            }
            if (data is double db) {
                return Hash (BitConverter.GetBytes (db));
            }
            if (data is float fl) {
                return Hash (BitConverter.GetBytes (fl));
            }
            var tos = data?.ToString ();
            return tos != null ? Hash (tos) : new byte [0];
        }
    }


    public class HashGenerator<T> : HashGenerator {

        public CopierOptions Option { get; protected set; }

        public HashGenerator () {
            this.Attribute = typeof (DataMemberAttribute);
            this.Option = CopierOptions.DataMember;
        }

        public HashGenerator (CopierOptions options)
            : this () {
            this.Option = options;
        }

        public Type Attribute { get; protected set; }
        public Func<PropertyInfo, bool> CustomMemberFilter { get; set; }

        protected virtual Func<PropertyInfo, bool> MemberFilter () {

            if (CustomMemberFilter != null)
                return CustomMemberFilter;

            if (Option == CopierOptions.DataMember) {
                return p => p != null && p.IsDefined (Attribute, true);

            } else if (Option == CopierOptions.ValueTypes) {
                return p => {
                    if (p == null) return false;
                    var type = p.PropertyType;
                    return type.IsPublic && p.CanWrite &&
                    (type.IsEnum || type.IsPrimitive || type.IsValueType || type == typeof (string));
                };
            }
            return p => false;
        }

        protected readonly MemberReflectionCache _dataMemberCache = new MemberReflectionCache ();
        protected readonly MemberReflectionCache _valueTypeCache = new MemberReflectionCache ();
        protected MemberReflectionCache Cache {
            get {
                if (this.Option == CopierOptions.DataMember)
                    return _dataMemberCache;
                else
                    return _valueTypeCache;
            }
        }

        protected readonly IDictionary<int, Delegate> copyActionCache = new Dictionary<int, Delegate> ();

        /// <summary>
        /// for prototyping, dont use, its slow(er)
        /// </summary>
        Func<T, string> GetDelegate0 (T source) {

            var clazz = typeof (T);
            var sourceType = source.GetType ();

            var sourceMembers = Cache.Members (sourceType, MemberFilter ());
            Func<T, string> result = d => {
                return HashString (sourceMembers.Select (p => Hash (p.GetValue (source))).ToArray ());
            };
            return result;

        }

        protected Func<T, string> GetDelegate (T source) {

            var clazz = typeof (T);
            var sourceType = source.GetType ();

            var key = KeyMaker.GetHashCode (clazz, sourceType);

            if (!copyActionCache.TryGetValue (key, out Delegate hashAction)) {
                var delegateType = typeof (Func<T, string>);
                var sourceExpr = Expression.Variable (sourceType, "source");

                var sourceMembers = Cache.Members (sourceType, MemberFilter ());

                var paras = new ParameterExpression [] { sourceExpr };
                var hashArguments = new List<Expression> ();

                Expression<Func<string, byte []>> hashMember = s => Hash (s);
                var gencall = (hashMember.Body as MethodCallExpression).Method.GetGenericMethodDefinition ();
                Expression<Func<byte [] [], string>> callHash = bytes => HashString (bytes);
                var hashParams = Expression.Parameter (typeof (byte []), "bytes");

                try {
                    foreach (var sourceInfo in sourceMembers.OrderBy (s => s.Name)) {

                        var sourceMember = Expression.Property (sourceExpr, sourceInfo);
                        var memberCall = Expression.Call (null, gencall.MakeGenericMethod (sourceInfo.PropertyType), sourceMember);
                        hashArguments.Add (memberCall);

                    }
                    var arr = Expression.NewArrayInit (typeof (byte []), hashArguments);
                    var call = Expression.Call ((callHash.Body as MethodCallExpression).Method, arr);
                    var hashExpression = Expression.Lambda (delegateType, call, paras);
                    hashAction = hashExpression.Compile ();

                } catch (Exception ex) {

                    throw ex;
                }
                copyActionCache.Add (key, hashAction);
            }
            return (Func<T, string>)hashAction;
        }

        /// <summary>
        /// md5-hash for type
        /// uses property names and types
        /// </summary>
        /// <returns>The hash.</returns>
        public string TypeHash () {

             var bytes = Cache.Members (typeof (T), MemberFilter ()).OrderBy (s => s.Name).SelectMany (m => new[] { Hash (m.Name), Hash (m.PropertyType) })
            .ToArray();
            return HashString (bytes);

        }
        /// <summary>
        /// md5-hash for type
        /// uses property index and types
        /// </summary>
        /// <returns>The hash.</returns>
        public string TypePropertyIndexHash () {
            var i = 1;
            var bytes = Cache.Members (typeof (T), MemberFilter ()).OrderBy (s => s.Name).SelectMany (m => new [] { Hash (i++), Hash (m.PropertyType) })
            .ToArray ();
            return HashString (bytes);
        }

        public string Hash (T source) {
            if (source == default) {
                return string.Empty;
            }

            var copyAction = GetDelegate (source);
            var md5 = copyAction.Invoke (source);
            return md5;
        }

        protected static IEqualityComparer comparer = EqualityComparer<object>.Default;

        public IEnumerable<PropertyInfo> Difference (T source, T target) {
            if (source == null || target == null)
                yield break;

            var sourceType = typeof (T);
            var desttype = typeof (T);
           
            foreach (var info in Cache.Members (typeof (T), MemberFilter ())) {
                var sourceValue = info.GetValue (source, null);
                if (Cache.ValidMember (desttype, info.PropertyType, info.Name)) {
                    var destValue = Cache.GetValue (desttype, info.PropertyType, info.Name, target);
                    if (!comparer.Equals (sourceValue, destValue))
                        yield return info;
                }

            }

        }

    }

}


