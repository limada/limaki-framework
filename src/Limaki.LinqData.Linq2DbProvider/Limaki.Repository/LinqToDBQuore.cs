/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common;
using Limaki.Common.Linqish;
using Limaki.Data;
using Limaki.Repository;
using LinqToDB;
using LinqToDB.Data;

namespace Limaki.Repository {

    public class LinqToDBQuore : IModeledQuore {

        public LinqToDBQuore (Iori iori) {
            Gateway = new LinqToDBGateway (iori);
        }

        public LinqToDBQuore (Iori iori, ILinqToDBModelBuilder builder) : this (iori) {
            ModelBuilder = builder;
        }

        public LinqToDBQuore (LinqToDBGateway gateway) {
            Gateway = gateway;
        }

        public LinqToDBQuore (LinqToDBGateway gateway, ILinqToDBModelBuilder builder) : this (gateway) {
            ModelBuilder = builder;
        }

        LinqToDBGateway Gateway { get; set; }

        IGateway IQuore.Gateway => this.Gateway;

        TextWriter _log = null;
        public TextWriter Log {
            get => _log ??= new StringWriter ();
            set => _log = value;
        }

        public ILinqToDBModelBuilder ModelBuilder { get; private set; }

        IDbModelBuilder IModeledQuore.ModelBuilder => ModelBuilder;

        protected void CheckModel<T> () {
            ModelBuilder?.CheckModel<T>(Gateway);
            ModelBuilder?.CheckTable<T>(Gateway);
            ModelBuilder?.CheckIndices<T> (Gateway);
        }

        private CallCache GetQueryCallCache = new CallCache (
        ExpressionUtils.Lambda<Func<LinqToDBQuore, IQueryable<CallCache.Entity>>> (c => c.GetQueryCall<CallCache.Entity> ())
        );

        private IQueryable<T> GetQueryCall<T> () where T : class => Gateway.Connection.GetTable<T> ();

        public virtual IQueryable<T> GetQuery<T> () {

            var entityType = typeof (T);
            if (!entityType.IsClass)
                throw new ArgumentException ();

            CheckModel<T> ();

            var d = (Func<LinqToDBQuore, IQueryable<T>>)GetQueryCallCache.Getter (entityType);
            return d (this);
        }

        private CallCache GetTableCallCache = new CallCache (
        ExpressionUtils.Lambda<Func<LinqToDBQuore, ITable<CallCache.Entity>>> (c => c.GetTableCall<CallCache.Entity> ()));

        private ITable<T> GetTableCall<T> () where T : class => Gateway.Connection.GetTable<T> ();

        public void Remove<T> (Expression<Func<T, bool>> where) {

            var entityType = typeof (T);
            if (!entityType.IsClass)
                throw new ArgumentException ();

            var d = (Func<LinqToDBQuore, IQueryable<T>>)GetQueryCallCache.Getter (entityType);
            var table = d (this);
            var deleted = table.Delete (where);
        }

        public void Remove<T> (IEnumerable<T> entities) {

            var entityType = typeof (T);
            if (!entityType.IsClass)
                throw new ArgumentException ();

            CheckModel<T> ();

            foreach (var e in entities) {
                Gateway.Connection.Delete (e);
            }
        }

        public void Insert<T> (IEnumerable<T> entities) {

            var entityType = typeof (T);
            if (!entityType.IsClass)
                throw new ArgumentException ();

            CheckModel<T> ();

            foreach (var e in entities) {
                Gateway.Connection.Insert (e);
            }
        }

        public void Upsert<T> (IEnumerable<T> entities) {

            var entityType = typeof (T);
            if (!entityType.IsClass)
                throw new ArgumentException ();

            CheckModel<T> ();

            foreach (var e in entities) {
                Gateway.Connection.InsertOrReplace (e);
            }
        }

        public IQuoreTransaction BeginTransaction () {
            var trans = new LinqToDBQuoreDbTransaction (Gateway.Connection);
            trans.BeginTransaction ();
            return trans;
        }

        public void EndTransaction (IQuoreTransaction transaction) {
            if (transaction is LinqToDBQuoreDbTransaction lt)
                lt.Commit ();
        }

        public void Dispose () {
            Gateway?.Dispose ();
            Gateway = null;
        }


    }

}
