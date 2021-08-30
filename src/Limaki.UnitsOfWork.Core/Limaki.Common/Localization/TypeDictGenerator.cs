/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Limaki.Common.Reflections;
using System;

namespace Limaki.Localizations {

    public class TypeDictGenerator {

        public enum MemberOptions {
            Public,
        }

        public MemberOptions MemberOption { get; set; } = MemberOptions.Public;

        IEnumerable<PropertyInfo> Members (Type type, Func<PropertyInfo, bool> memberFilter) {
            if (type.IsInterface) {
                var result = cache.Members (type, memberFilter).ToList ();
                foreach (var item in type.GetInterfaces ())
                    result.AddRange (cache.Members (item, memberFilter));
                return result;
            } else {
                return cache.Members (type, memberFilter);
            }
        }

        public string ClassName (Type type) {
            var result = type.Name;
            if (type.IsNested && !type.IsGenericParameter) {
                result = type.FullName.Replace (type.DeclaringType.FullName + "+", ClassName (type.DeclaringType));
            }

            if (type.IsGenericType) {

                var genPos = result.IndexOf ('`');
                if (genPos > 0)
                    result = result.Substring (0, genPos);
                else
                    result = result + "";
                bool isNullable = result == "Nullable";
                if (!isNullable)
                    result += "<";
                else
                    result = "";
                foreach (var item in type.GetGenericArguments ())
                    result += ClassName (item) + ",";
                result = result.Remove (result.Length - 1, 1);
                if (isNullable)
                    result += "?";
                else
                    result += ">";
            }

            return result;
        }

        private static readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Instance |
                                                             BindingFlags.DeclaredOnly;
        //| BindingFlags.FlattenHierarchy;

        private static readonly MemberReflectionCache _dataMemberCache = new MemberReflectionCache (_bindingFlags);
        private static readonly MemberReflectionCache _valueTypeCache = new MemberReflectionCache (_bindingFlags);
        private static readonly MemberReflectionCache _publicCache = new MemberReflectionCache (_bindingFlags);
        private static readonly MemberReflectionCache _tableField = new MemberReflectionCache (_bindingFlags);

        private MemberReflectionCache cache {
            get {
                if (this.MemberOption == MemberOptions.Public)
                    return _publicCache;

                return _valueTypeCache;
            }
        }

        protected Func<PropertyInfo, bool> MemberFilter () {
            if (MemberOption == MemberOptions.Public) {
                return (p) => {
                    if (p == null) return false;
                    var type = p.PropertyType;
                    return type.IsPublic;
                };
            }

            return (p) => false;
        }

        HashSet<string> done = new HashSet<string> ();
        StringBuilder s = new StringBuilder ();

        public string Result { get { return s.ToString (); } }

        public virtual void AddClass (Type type) {

            void Add (string st) {
                if (done.Contains (st)) return;
                s.AppendLine ($"\"{st}\",\"\"");
                done.Add (st);
            }

            var memberFilter = MemberFilter ();
            cache.AddType (type, memberFilter);
            var members = Members (type, memberFilter).OrderBy (i => i.Name);
            Add (ClassName (type));

            foreach (var info in members) {
                Add (info.Name);
            }
        }
    }

}