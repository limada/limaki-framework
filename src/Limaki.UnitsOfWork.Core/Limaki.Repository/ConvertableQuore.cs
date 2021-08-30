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

namespace Limaki.Repository {

    public class ConvertableQuore : IQuore, IConvertableQuore {

        public IQuore InnerQuore { get; set; }
        public Func<Expression, Type, Expression> Convert { get; set; }

        public ConvertableQuore (IQuore innerQuore) {
            InnerQuore = innerQuore;
        }

        public ConvertableQuore (IQuore innerQuore, Func<Expression, Type, Expression> convert):this(innerQuore) {
            Convert = convert;
        }

        public IGateway Gateway {
            get { return InnerQuore.Gateway; }
        }

        public System.IO.TextWriter Log {
            get { return InnerQuore.Log; }
            set { InnerQuore.Log = value; }
        }

        public virtual IQueryable<T> GetQuery<T> () {
            if (Convert != null)
                return new ConvertableQuery<T> (InnerQuore.GetQuery<T> (), Convert);
            else
                return InnerQuore.GetQuery<T> ();
        }

        public virtual void Insert<T> (IEnumerable<T> entities) {
            InnerQuore.Insert (entities);
        }

        public virtual void Upsert<T> (IEnumerable<T> entities) {
            InnerQuore.Upsert (entities);
        }

        public virtual void Remove<T> (IEnumerable<T> entities) {
            InnerQuore.Remove (entities);
        }

        public virtual void Remove<T> (Expression<Func<T, bool>> where) {
            InnerQuore.Remove<T> (where);
        }

        public virtual void Dispose () {
            InnerQuore.Dispose ();
        }

        public IQuoreTransaction BeginTransaction () {
            return InnerQuore.BeginTransaction ();
        }

        public void EndTransaction (IQuoreTransaction transaction) {
            InnerQuore.EndTransaction (transaction);
        }
    }
}