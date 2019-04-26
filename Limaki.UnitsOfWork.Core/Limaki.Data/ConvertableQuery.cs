/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Limaki.Data {

    /// <summary>
    /// An IQueryable wrapper that allows to convert the query's expression tree before the inner provider gets it
    /// based on LinqKit.ExpandableQuery (https://github.com/scottksmith95/LINQKit)
    /// </summary>
    public class ConvertableQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IOrderedQueryable {
        readonly ConvertableQueryProvider<T> _provider;
        readonly IQueryable<T> _inner;
        internal IQueryable<T> InnerQuery { get { return _inner; } } 
        /// <summary>
        /// conversion operation
        /// </summary>
        protected Func<Expression, Type, Expression> Convert { get; set; }
        public ConvertableQuery (IQueryable<T> inner, Func<Expression, Type, Expression> convert) {
            _inner = inner;
            Convert = convert;
            _provider = new ConvertableQueryProvider<T> (this, convert);
        }

        Expression IQueryable.Expression {
            get { return _inner.Expression; }
        }

        Type IQueryable.ElementType { get { return typeof (T); } }

        IQueryProvider IQueryable.Provider { get { return _provider; } }

        public IEnumerator<T> GetEnumerator () { return _inner.GetEnumerator (); }

        IEnumerator IEnumerable.GetEnumerator () { return _inner.GetEnumerator (); }

        public override string ToString () { return _inner.ToString (); }

    }

    public class ConvertableQueryProvider<T> : IQueryProvider {

        readonly ConvertableQuery<T> _query;

        public ConvertableQueryProvider (ConvertableQuery<T> query, Func<Expression, Type, Expression> convert) {
            _query = query;
            Convert = convert;
        }

        protected Func<Expression, Type, Expression> Convert { get; set; }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement> (Expression expression) {
            var exp = Convert (expression, typeof (TElement));
            return new ConvertableQuery<TElement> (_query.InnerQuery.Provider.CreateQuery<TElement> (exp), Convert);
        }

        TResult IQueryProvider.Execute<TResult> (Expression expression) {
            return _query.InnerQuery.Provider.Execute<TResult> (Convert (expression, typeof (TResult)));
        }

        IQueryable IQueryProvider.CreateQuery (Expression expression) {
            return _query.InnerQuery.Provider.CreateQuery (Convert (expression, typeof (T)));
        }

        object IQueryProvider.Execute (Expression expression) {
            return _query.InnerQuery.Provider.Execute (Convert (expression, typeof (T)));
        }

    }
}