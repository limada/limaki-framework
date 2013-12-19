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
using System.Reflection;

namespace Limaki.Common.UnitsOfWork {
    [DataContract]
    public class ListContainer: IDisposable {
        public IEnumerable<Type> KnownTypes() {
            var result = new List<Type>();
            var genType = typeof(IEnumerable<>).GetGenericTypeDefinition();
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

        protected static readonly IDictionary<Type, PropertyInfo> _listProperties = new Dictionary<Type, PropertyInfo>();
        protected virtual PropertyInfo ListProperty<T>() {
            PropertyInfo list = null;
            //if (!_listProperties.TryGetValue(type, out list)) {
            list = this.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(IEnumerable<T>))
                .FirstOrDefault();
            //_listProperties.Add(type, list);
            //}
            return list;
        }

      

        public virtual void Set<T>(IEnumerable<T> value) {
             var list = ListProperty<T>();
             if (list != null) {
                 list.SetValue(this, value, null);
             } else {
                 throw new ArgumentException(this.GetType() + " has no " + typeof (T).Name + " IEnumerable-Property");
             }
        }

        public virtual bool HasList<T>() {
            return ListProperty<T>() != null;
        }

        public virtual IEnumerable<T> List<T>() {
            var list = ListProperty<T>();
            if (list != null) {
                var result = list.GetValue(this, null) as IEnumerable<T>;
                if (result == null) {
                    result = new List<T>();
                    list.SetValue(this, result, null);
                }
                return result;
            } 

            throw new ArgumentException(this.GetType() + " has no " + typeof (T).Name + " IEnumerable-Property");
         
        }

        public virtual void Clear() {
            foreach (var propertyInfo in this.GetType().GetProperties()
                .Where(p =>
                     p.PropertyType.IsGenericType &&
                     p.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))) {
                var list = propertyInfo.GetValue(this, null) as System.Collections.IList;
                if (list != null)
                    list.Clear();
                propertyInfo.SetValue(this, null, null);
            }
        }

        public virtual void Dispose(bool disposing) {
            Clear();
        }

        public virtual void Dispose() {
            Dispose(true);
        }

        ~ListContainer() {
            Dispose(false);
        }
    }
}