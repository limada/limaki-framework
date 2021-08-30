/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {

    /// <summary>
    /// Criterias for <see cref="IQueryable{T}"/>
    /// </summary>
    public interface ICriterias : IPaged {
        
        Guid MainQuery { get; set; }
        GuidFlags Resolve { get; set; }

        IEnumerable<LambdaExpression> GetExpressions();
        LambdaExpression SetExpression(LambdaExpression exp);

        IQueryable<T> ApplyExpression<T> (IQueryable<T> query, params Expression<Func<T, bool>>[] exps);
        IQueryable<T> ApplyExpression<T> (IQueryable<T> query, IEnumerable<Expression<Func<T, bool>>> exps, bool orElse = false);
        IEnumerable<T> ApplyExpression<T> (IEnumerable<T> query, IEnumerable<Expression<Func<T, bool>>> exps, bool orElse = false);
        IQueryable<T> ApplyExpressionOr<T> (IQueryable<T> query, params Expression<Func<T, bool>>[] exps);

        Expression<Func<T, bool>> CombineExpressions<T> (params Expression<Func<T, bool>>[] exps);
        Expression<Func<T, bool>> CombineExpressions<T> (IEnumerable<Expression<Func<T, bool>>> exps, bool orElse = false);
        Expression<Func<T, bool>> CombineExpressionsOr<T> (params Expression<Func<T, bool>>[] exps);
        Expression<Func<T, bool>> SetExpression<T> (Expression<Func<T, bool>> exp);

        string ToCSharpCode ();
    }
}