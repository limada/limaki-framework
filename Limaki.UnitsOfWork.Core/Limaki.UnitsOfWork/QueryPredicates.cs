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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using System.Runtime.Serialization;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {

    [DataContract]
    public class QueryPredicates : IQueryPredicates {

        public virtual Guid MainQuery { get; set; }
        public virtual GuidFlags Resolve { get; set; }

        public Paging Paging { get; set; }

        public string ToCSharpCode () {
            var type = this.GetType ();
            using (var writer = new StringWriter ()) {
                foreach (var prop in type.GetProperties ().Where (p => typeof (Expression).IsAssignableFrom (p.PropertyType))) {
                    var exp = prop.GetValue (this, null) as Expression;
                    if (exp != null)
                        writer.WriteLine (exp.ToCSharpCode ().Replace ("\r\n", " ").Replace ('\t', ' ').Replace ("  ", " "));
                }
                return writer.ToString ();
            }
        }

        public virtual Expression<Func<T, bool>> SetExpression<T> (Expression<Func<T, bool>> exp) {
            var expType = exp.GetType ();
            var prop = GetType ().GetProperties ().Where (p => p.PropertyType.IsAssignableFrom(expType)).FirstOrDefault ();
            if (prop == null)
                throw new ArgumentException ($"{this.GetType()}: {exp} not supported");
            prop.SetValue (this, exp);
            return exp;
        }

        public virtual IQueryable<T> ApplyExpression<T> (IQueryable<T> query, params Expression<Func<T, bool>> [] exps) {
            return ApplyExpression (query, exps, false) as IQueryable<T>;
        }

        public virtual IQueryable<T> ApplyExpressionOr<T> (IQueryable<T> query, params Expression<Func<T, bool>> [] exps) {
            return ApplyExpression (query, exps, true) as IQueryable<T>;
        }

        public virtual IQueryable<T> ApplyExpression<T> (IQueryable<T> query, IEnumerable<Expression<Func<T, bool>>> exps, bool orElse = false) {
            return ApplyExpression ((IEnumerable<T>)query, exps, orElse) as IQueryable<T>;
        }

        public virtual Expression<Func<T, bool>> CombineExpressions<T> (params Expression<Func<T, bool>> [] exps) {
            return CombineExpressions ((IEnumerable<Expression<Func<T, bool>>>)exps);
        }

        public virtual Expression<Func<T, bool>> CombineExpressionsOr<T> (params Expression<Func<T, bool>> [] exps) {
            return CombineExpressions (exps, true);
        }

        public virtual Expression<Func<T, bool>> CombineExpressions<T> (IEnumerable<Expression<Func<T, bool>>> exps, bool orElse = false) {
            exps = exps.Where (e => e != null);
            var exp = exps.FirstOrDefault ();
            if (exp != null) {
                foreach (var f in exps.Skip (1))
                    if (orElse)
                        exp = exp.OrElse (f);
                    else
                        exp = exp.AndAlso (f);
            }
            return exp;
        }

        public virtual IEnumerable<T> ApplyExpression<T> (IEnumerable<T> query, IEnumerable<Expression<Func<T, bool>>> exps, bool orElse = false) {
            if (!exps.GetType ().IsArray)
                exps = exps.ToArray ();

            var exp = CombineExpressions (exps, orElse);
            if (exp != null) {
                var q = query as IQueryable<T>;
                if (q != null) {
                    query = q.Where (exp);
                } else {
                    query = query.Where (exp.Compile ());
                }
            }
            return query;
        }

    }


}