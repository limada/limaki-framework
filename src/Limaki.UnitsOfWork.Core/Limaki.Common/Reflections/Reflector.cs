/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.Common.Collections;
using System.Reflection;
using System.Text;
using System.Linq;
using Limaki.Common.Text;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Limaki.Common.Reflections {

    public static class Reflector {

        public static bool IsStorable (Type type) {
            return !(type.IsPrimitive || type == typeof(string));
        }

        public static bool IsSimple (this Type type) => type != null && type.IsPrimitive ||
                                                        type == typeof(string) ||
                                                        type == typeof(decimal) ||
                                                        type == typeof(DateTime);

        public static bool IsSimpleOrNullable (this Type type) => type != null && (Nullable.GetUnderlyingType (type).IsSimple () || type.IsSimple ());

        static Set<Tuple<Type, Type>> _implements = new Set<Tuple<Type, Type>> ();

        public static bool Implements (Type clazz, Type interfaze) {
            if (clazz.IsClass) {
                var key = Tuple.Create (clazz, interfaze);

                if (_implements.Contains (key))
                    return true;

                var result = (interfaze.IsAssignableFrom (clazz));

                if (!result && interfaze.IsInterface) {
                    foreach (Type t in clazz.GetInterfaces ()) {
                        if (t == interfaze) {
                            result = true;
                            _implements.Add (key);

                            break;
                        }
                    }
                }

                return result;
            } else {
                return false;
            }
        }

        /// <summary>
        /// gives back the name of integral types
        /// </summary>
        /// <returns>The integral type name.</returns>
        /// <param name="type">Type.</param>
        public static string FriendlyIntegralTypeName (this Type type) {
            switch (type) {
                case Type t when t == typeof(Int16):
                    return "short";
                case Type t when t == typeof(Int32):
                    return "int";
                case Type t when t == typeof(Int64):
                    return "long";
                case Type t when t == typeof(UInt16):
                    return "ushort";
                case Type t when t == typeof(UInt32):
                    return "uint";
                case Type t when t == typeof(UInt64):
                    return "ulong";
                case Type t when t == typeof(Single):
                    return "float";
                case Type t when t == typeof(Boolean):
                    return "bool";
                case Type t when
                    t == typeof(Object) ||
                    t == typeof(String) ||
                    t == typeof(Byte) ||
                    t == typeof(Char) ||
                    t == typeof(Decimal) ||
                    t == typeof(SByte) ||
                    t == typeof(Double) ||
                    t == typeof(void)
                    :
                    return t.Name.ToLower ();
                default:
                    return type.Name;
            }
        }

        public static bool IsTuple (this Type type) =>
            type.GetInterfaces ().Any (i => i.Name == "ITuple"
                                            && i.Namespace == typeof(IsLong).Namespace);

        public static bool IsNullable (this Type type) => Nullable.GetUnderlyingType (type) != null;

        public static string FriendlyClassName (this Type type) {
            var result = new StringBuilder ();

            if (type.IsNested && !type.IsGenericParameter) {
                result.Append (type.FullName.Replace (type.DeclaringType.FullName + "+", FriendlyClassName (type.DeclaringType) + "."));
            } else {
                result.Append (FriendlyIntegralTypeName (type));
            }

            if ((type.IsGenericType || type.ContainsGenericParameters) && !type.IsGenericParameter) {

                var genPos = result.IndexOf ("`");

                if (genPos > 0)
                    result = result.Remove (genPos, result.Length - genPos);

                var isNullable = type.IsNullable ();
                var isTuple = type.IsTuple ();

                var genDelim = isNullable ? "" : isTuple ? "(" : "<";

                if (isNullable || isTuple) {
                    result.Clear ();
                }

                result.Append (genDelim);

                result.Append (string.Join (",", type.GetGenericArguments ().Select (t => t.FriendlyClassName ())));

                genDelim = isNullable ? "?" : isTuple ? ")" : ">";
                result.Append (genDelim);
            }

            return result.ToString ();
        }

        private static A GetSingleAttribute<A> (object[] attributes) where A : Attribute =>
            attributes.Length > 0 ? (A) attributes[0] : default;

        /// <summary>
        /// Returns a requested attribute for a given type
        /// </summary>
        /// <typeparam name="A">The requested attribute type</typeparam>
        /// <param name="t">The class supposed to provide that attribute</param>
        /// <returns>An attribute of type A or null if none</returns>
        public static A GetAttribute<A> (this Type t) where A : Attribute =>
            GetSingleAttribute<A> (t.GetCustomAttributes (typeof(A), true));

        /// <summary>
        /// Returns a requested attribute for a given member
        /// </summary>
        /// <typeparam name="A">The requested attribute type</typeparam>
        /// <param name="m">The member supposed to provide that attribute</param>
        /// <returns>An attribute of type A or null if none</returns>
        public static A GetAttribute<A> (this System.Reflection.MemberInfo m) where A : Attribute =>
            GetSingleAttribute<A> (m.GetCustomAttributes (typeof(A), true));

        public static A GetAttribute<A> (this System.Reflection.MethodInfo m) where A : Attribute =>
            GetSingleAttribute<A> (m.GetCustomAttributes (typeof(A), true));

        public static PropertyInfo[] GetPublicProperties (this Type type, BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) {
            if (type.IsInterface) {
                var propertyInfos = new List<PropertyInfo> ();

                var considered = new List<Type> ();
                var queue = new Queue<Type> ();
                considered.Add (type);
                queue.Enqueue (type);

                while (queue.Count > 0) {
                    var subType = queue.Dequeue ();

                    foreach (var subInterface in subType.GetInterfaces ()) {
                        if (considered.Contains (subInterface)) continue;

                        considered.Add (subInterface);
                        queue.Enqueue (subInterface);
                    }

                    var typeProperties = subType.GetProperties (flags); //BindingFlags.FlattenHierarchy| BindingFlags.Public| BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                       .Where (x => !propertyInfos.Contains (x));

                    propertyInfos.InsertRange (0, newPropertyInfos);
                }

                return propertyInfos.ToArray ();
            }

            return type.GetProperties (flags);
        }

        public static bool IsExtensionMethod (this MethodBase methodBase) => methodBase.GetCustomAttribute<ExtensionAttribute> () != null;

        public static Type EnumerableElement (this Type t) {
            if (t.IsSimpleOrNullable ())
                return default;

            if (t.IsArray)
                return t.GetElementType ();

            if (t.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom (t)) {
                return t.GenericTypeArguments.FirstOrDefault ();
            }

            return default;
        }

        public static bool IsRelation (this MemberInfo it) => it.Relation () != null;

        public static RelationAttribute Relation (this MemberInfo it) => it.GetCustomAttribute<RelationAttribute> ();

        /// <summary>
        /// Determines whether the specified types are considered equal.
        /// </summary>
        /// <param name="parent">A <see cref="System.Type"/> instance. </param>
        /// <param name="child">A type possible derived from the <c>parent</c> type</param>
        /// <returns>True, when an object instance of the type <c>child</c>
        /// can be used as an object of the type <c>parent</c>; otherwise, false.</returns>
        /// <remarks>Note that nullable types does not have a parent-child relation to it's underlying type.
        /// For example, the 'int?' type (nullable int) and the 'int' type
        /// aren't a parent and it's child.</remarks>
        public static bool IsSameOrParentOf (this Type parent, Type child) {
            if (parent == null) throw new ArgumentNullException (nameof(parent));
            if (child == null) throw new ArgumentNullException (nameof(child));

            if (parent == child ||
                child.IsEnum && Enum.GetUnderlyingType (child) == parent ||
                child.IsSubclassOf (parent)) {
                return true;
            }

            if (parent.IsGenericTypeDefinition)
                for (var t = child; t != typeof(object) && t != null; t = t.BaseType)
                    if (t.IsGenericType && t.GetGenericTypeDefinition () == parent)
                        return true;

            if (parent.IsInterface) {
                var interfaces = child.GetInterfaces ();

                foreach (var t in interfaces) {
                    if (parent.IsGenericTypeDefinition) {
                        if (t.IsGenericType && t.GetGenericTypeDefinition () == parent)
                            return true;
                    } else if (t == parent)
                        return true;
                }
            }

            return false;
        }

        public static bool IsGenericEnumerableType (this Type type) {
            if (type.IsGenericType)
                if (typeof(IEnumerable<>).IsSameOrParentOf (type))
                    return true;

            return false;
        }

        public static bool IsGenericQueryableType (this Type type) {
            if (type.IsGenericType)
                if (typeof(IQueryable<>).IsSameOrParentOf (type))
                    return true;

            return false;
        }
    }

}