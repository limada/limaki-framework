/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Data;

namespace Limaki.Repository {

    public class InMemoryQuore : IQuore {

        private Dictionary<Type, object> _lists = new Dictionary<Type, object> ();

        protected ICollection<T> GetList<T> () {
            var type = typeof (T);
            if (_lists.ContainsKey (type))
                return _lists[type] as ICollection<T>;
            var list = new List<T> ();
            _lists.Add (type, list);
            return list;
        }

        public IQueryable<T> GetQuery<T> () {
            return GetList<T> ().AsQueryable<T> ();

        }

        public void Insert<T> (IEnumerable<T> entities) {
            foreach (var e in entities)
                GetList<T> ().Add (e);
        }

        public void Upsert<T> (IEnumerable<T> entities) {
            foreach (var e in entities)
                GetList<T> ().Add (e);
        }

        public void Remove<T> (IEnumerable<T> entities) {
            foreach (var e in entities)
                GetList<T> ().Remove (e);
        }

        public void Remove<T> (Expression<Func<T, bool>> where) {
            var ent = GetQuery<T> ().Where (where).ToArray ();
            Remove (ent);
        }

        public void Dispose () {

        }

        public IQuoreTransaction BeginTransaction () {
            return new InMemoryTransaction ();
        }

        public void EndTransaction (IQuoreTransaction transaction) {
            
        }

        public IGateway Gateway { get; set; }

        TextWriter _log = null;
        public TextWriter Log {
            get { return _log ?? (_log = new StringWriter ()); }
            set { _log = value; }
        }

        public class InMemoryTransaction : IQuoreTransaction {
            
            public void Commit () { }

            public void Dispose () { }

            public void Rollback () { }
        }
    }
}