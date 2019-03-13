using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {
    
    public interface IQueryPredicates {
        
        Guid MainQuery { get; set; }
        GuidFlags Resolve { get; set; }

        Paging Paging { get; set; }

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