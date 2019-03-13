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

namespace Limaki.UnitsOfWork {

    [DataContract]
    public class ChangeSetContainer : IDisposable {
        
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
            return $"{changeSet.Created.Count} created \t {changeSet.Updated.Count} updated \t {changeSet.Removed.Count} removed";
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

        public bool HasData {
            get {
                foreach (var changeSetProperty in ChangeSetProperties ()) {
                    if (changeSetProperty.GetValue (this, null) is ChangeSet changeSet && changeSet.HasData) {
                        return true;
                    }
                }
                return false;
            }
        }

        public virtual void Clear() {
            foreach (var changeSetProperty in ChangeSetProperties()) {
                if (changeSetProperty.GetValue (this, null) is UnitsOfWork.ChangeSet changeSet) {
                    changeSet.Clear ();
                }
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    Clear ();
                }

                disposedValue = true;
            }
        }

        public void Dispose () {
            Dispose (true);

        }

    }
}