/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;

namespace Limaki.Common.UnitsOfWork {

    [DataContract]
    public class ChangeSetContainer {
        public IEnumerable<Type> KnownTypes() {
            var result = new List<Type>();
            var genType = typeof(ChangeSet<>).GetGenericTypeDefinition();
            foreach (var prop in this.GetType().GetProperties()) {
                if (prop.PropertyType.IsGenericType) {
                    var gettype = prop.PropertyType.GetGenericTypeDefinition();
                    if (gettype.Equals(genType)) {
                        result.Add(prop.PropertyType);
                    }
                }
            }
            return result;
        }

        public virtual ChangeSet<T> ChangeSet<T>() {
            var type = typeof(T);
            var changeSet = this.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(ChangeSet<T>))
                .FirstOrDefault();
            if (changeSet != null) {
                var result = changeSet.GetValue(this, null) as ChangeSet<T>;
                if (result == null) {
                    result = new ChangeSet<T>();
                    changeSet.SetValue(this, result, null);
                }
                return result;
            }
            return null;
        }

        public string ChangeSetInfo<T>(object value) {
            var changeSet = (ChangeSet<T>)value;
            return string.Format("{0} created \t {1} updated \t {2} removed ",
                                 changeSet.Created.Count, changeSet.Updated.Count,
                                 changeSet.Removed.Count);
        }

        public virtual IEnumerable<PropertyInfo> ChangeSetProperties() {
            var changeSets = this.GetType().GetProperties()
               .Where(p =>
                   p.PropertyType.IsGenericType &&
                   p.PropertyType.GetGenericTypeDefinition() == typeof(ChangeSet<>));

            foreach (var changeSetProperty in changeSets) {
                if (changeSetProperty != null) {

                    yield return changeSetProperty;
                }
            }
        }


        public virtual string Info() {
            var writer = new StringWriter();
            writer.WriteLine(this.GetType().Name);
            foreach (var changeSetProperty in ChangeSetProperties()) {
                var changeSet = changeSetProperty.GetValue(this, null);
                if (changeSet != null) {
                    var infomethod = this.GetType().GetMethod("ChangeSetInfo");
                    var method =
                        infomethod.MakeGenericMethod(changeSetProperty.PropertyType.GetGenericArguments().First());
                    writer.Write("\t" + changeSetProperty.Name + ":\t ");
                    writer.WriteLine(method.Invoke(this, new object[] { changeSet }));
                }
            }
            return writer.ToString();
        }

        public virtual void Clear() {
            foreach (var changeSetProperty in ChangeSetProperties()) {
                var changeSet = changeSetProperty.GetValue(this, null);
                if (changeSet != null) {
                    var method = changeSetProperty.PropertyType.GetMethod("Clear");
                    method.Invoke(changeSet,null);
                }
            }
        }
    }
}