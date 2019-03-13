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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Limaki.Common.Reflections;

namespace Limaki.Common.FunctionPoints
{

	public class FunctionPointCounter {
		
        public enum MemberOptions {
            Public,
            DataMember,
            ValueTypes,
            TableColumn
        }

        public void Header() {
            Write("{0}\t{1}\t{2}\t", "Name", "Nr of Members", "Namespace", "interface");
        }

        IEnumerable<PropertyInfo> Members(Type type, Func<PropertyInfo, bool> memberFilter) {
            if (type.IsInterface) {
                var result = cache.Members(type, memberFilter).ToList();
                foreach (var item in type.GetInterfaces())
                    result.AddRange(cache.Members(item, memberFilter));
                return result;
            } else {
                return cache.Members(type, memberFilter);
            }
        }

        public int Points { get; set; }

        public virtual string ClassName(Type type) {
            var result = type.Name;
            if(type.IsNested && !type.IsGenericParameter) {
                result = type.FullName.Replace(type.DeclaringType.FullName+"+", ClassName(type.DeclaringType));
            }
            if (type.IsGenericType) {
                
                var genPos = result.IndexOf('`');
                if (genPos > 0)
                    result = result.Substring(0, genPos);
                else
                    result = result + "";
                bool isNullable = result == "Nullable";
                if (!isNullable)
                    result += "<";
                else
                    result = "";
                foreach (var item in type.GetGenericArguments())
                    result += ClassName(item)+",";
                result = result.Remove(result.Length - 1, 1);
                if (isNullable)
                    result += "?";
                else
                    result += ">";
            } 
            return result;
        }

        public virtual void ReportClass<T>() {
            ReportClass(typeof(T));
        }

        public bool ReportMembers { get; set; }

		public virtual void ReportClass(Type type)
		{


			var memberFilter = MemberFilter();
			cache.AddType(type, memberFilter);
			var members = Members(type, memberFilter).OrderBy(i => i.Name);
			var count = members.Count() + 1;
			foreach (var info in members)
			{
				var isRelation = info.GetCustomAttributes().Any(e => e.GetType() == typeof(RelationAttribute));
				var isView = info.GetCustomAttributes().Any(e => e.GetType() == typeof(DisplayAttribute));
				var isEnumerable = isRelation && typeof(System.Collections.IEnumerable).IsAssignableFrom(info.PropertyType);
				if (isRelation)
					count += 1;
				if (isEnumerable)
					count += 1;
				if (isView)
					count += 1;
			}
			WriteLine($"{ClassName(type)}\t\t{count}\t{type.Namespace}\t{(type.IsInterface ? "interface" : "class")}");

			this.Points += count;
			if (ReportMembers)
				foreach (var info in members)
				{
					var memberCount = 1;
					var isRelation = info.GetCustomAttributes().Any(e => e.GetType() == typeof(RelationAttribute));
					var isEnumerable = isRelation && typeof(System.Collections.IEnumerable).IsAssignableFrom(info.PropertyType);
					var isView = info.GetCustomAttributes().Any(e => e.GetType() == typeof(DisplayAttribute));
					if (isRelation)
						memberCount += 1;
					if (isEnumerable)
						memberCount += 1;
					if (isView)
						memberCount += 1;
				WriteLine($"\t{info.Name}\t{memberCount}\t{ClassName(info.PropertyType)}\t{(isRelation ? "Relation" : "")}\t{(isView ? "View" : "")}");

				}
			WriteLine("");
		}

        private MemberOptions _memberOption = MemberOptions.Public;
        public MemberOptions MemberOption {
            get { return _memberOption; }
            protected set {
                if (value != _memberOption) {
                    _memberOption = value;
                    if (value == MemberOptions.DataMember)
                        this.Attribute = typeof(DataMemberAttribute);

					}

            }
        }

        public FunctionPointCounter() { }

        public FunctionPointCounter(MemberOptions memberOptions): this() {
            this.MemberOption = memberOptions;
        }

        public Type Attribute { get; protected set; }

        protected Func<PropertyInfo, bool> MemberFilter() {
            if (MemberOption == MemberOptions.DataMember || MemberOption == MemberOptions.TableColumn) {
                return (p) => p != null && p.IsDefined(Attribute, true);
            } else if (MemberOption == MemberOptions.Public) {
                return (p) => {
                           if (p == null) return false;
                           var type = p.PropertyType;
                           return type.IsPublic;
                       };
            } else if (MemberOption == MemberOptions.ValueTypes) {
                return (p) => {
                           if (p == null) return false;
                           var type = p.PropertyType;
                           return type.IsPublic &&
                                  (type.IsEnum || type.IsPrimitive || type.IsValueType);
                       };
            }
            return (p) => false;
        }

        private static readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.Instance |
                                                             BindingFlags.DeclaredOnly;
                                                             //| BindingFlags.FlattenHierarchy;

        private static readonly MemberReflectionCache _dataMemberCache = new MemberReflectionCache(_bindingFlags);
        private static readonly MemberReflectionCache _valueTypeCache = new MemberReflectionCache(_bindingFlags);
        private static readonly MemberReflectionCache _publicCache = new MemberReflectionCache(_bindingFlags);
        private static readonly MemberReflectionCache _tableField = new MemberReflectionCache(_bindingFlags);
        private MemberReflectionCache cache {
            get {
                if (this.MemberOption == MemberOptions.DataMember)
                    return _dataMemberCache;
                else if (this.MemberOption == MemberOptions.Public)
                    return _publicCache;
                else if (this.MemberOption == MemberOptions.TableColumn)
                    return _tableField;
                else
                    return _valueTypeCache;
            }
        }

        #region Write
        public TextWriter Writer { get; set; }
        public void WriteLine(string message, params object[] args) {
            if (Writer != null) {
                Writer.WriteLine(string.Format(message, args));
            } else {
                Trace.WriteLine(string.Format(message, args));
            }
        }

        public void Write(string message, params object[] args) {
            if (Writer != null) {
                Writer.Write(string.Format(message, args));
            } else {
                Trace.Write(string.Format(message, args));
            }
        }

        #endregion

       


    }
}