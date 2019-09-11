/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 - 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Data;

namespace Limaki.LinqData {

    public class ConvertableQuore : IQuore, IConvertableQuore {

        public IQuore InnerStore { get; set; }
        public Func<Expression, Type, Expression> Convert { get; set; }

        public ConvertableQuore (IQuore innercontext) {
            InnerStore = innercontext;
        }

        public ConvertableQuore (IQuore innercontext, Func<Expression, Type, Expression> convert):this(innercontext) {
            Convert = convert;
        }

        public IGateway Gateway {
            get { return InnerStore.Gateway; }
        }

        public System.IO.TextWriter Log {
            get { return InnerStore.Log; }
            set { InnerStore.Log = value; }
        }

        public virtual IQueryable<T> GetQuery<T> () {
            if (Convert != null)
                return new ConvertableQuery<T> (InnerStore.GetQuery<T> (), Convert);
            else
                return InnerStore.GetQuery<T> ();
        }

        public virtual void Insert<T> (IEnumerable<T> entities) {
            InnerStore.Insert (entities);
        }

        public virtual void Upsert<T> (IEnumerable<T> entities) {
            InnerStore.Upsert (entities);
        }

        public virtual void Remove<T> (IEnumerable<T> entities) {
            InnerStore.Remove (entities);
        }

        public virtual void Remove<T> (Expression<Func<T, bool>> where) {
            InnerStore.Remove<T> (where);
        }

        public virtual void Dispose () {
            InnerStore.Dispose ();
        }

        public IQuoreTransaction BeginTransaction () {
            return InnerStore.BeginTransaction ();
        }

        public void EndTransaction (IQuoreTransaction transaction) {
            InnerStore.EndTransaction (transaction);
        }
    }
}