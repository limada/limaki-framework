/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 - 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Common.Linqish;
using Limaki.Common.Reflections;

namespace Limaki.UnitsOfWork.Data {

    public abstract class EntityRepositoryCounter<TQuore, TPredicates, TCounts, TMapper> : EntityRepository
      where TCounts : EntityCounts, new() where TQuore : IEntityQuore where TPredicates : QueryPredicates where TMapper : EntityQuoreMapper, new() {

        public long Count<T> (IQueryable<T> query, Expression<Func<T, bool>> where) => query.Where (where).LongCount ();

        protected CallCache CountCallCache => new CallCache (ExpressionUtils.Lambda
          <Func<EntityRepositoryCounter<TQuore, TPredicates, TCounts, TMapper>, IQueryable<CallCache.Entity>, Expression<Func<CallCache.Entity, bool>>, long>>
             ((counter, querable, predicate) => counter.Count (querable, predicate)));

        EntityQuoreMapper Mapper = new TMapper ();

        public TCounts Count (TPredicates preds, TQuore quore) {
            Log.Debug (nameof (Count));
            var result = new TCounts ();

            foreach (var predicateProp in preds.GetType ().GetProperties ().Where (p => p.PropertyType.BaseType == typeof (LambdaExpression))) {
                var predicate = predicateProp.GetValue (preds) as LambdaExpression;
                if (predicate != null) {
                    Log.Debug (predicate.ToString ());
                    var elementType = Mapper.MapIn (predicate.Type.GenericTypeArguments[0]);
                    predicate = Mapper.Map (predicate, elementType) as LambdaExpression;
                    var queryableProperty = quore.GetType ().GetProperties ()
                                 .Where (p => p.PropertyType.IsGenericType
                                         && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>)
                                         && p.PropertyType.GenericTypeArguments[0] == elementType)
                                 .FirstOrDefault ();
                    if (queryableProperty != null) {
                        var queryable = queryableProperty.GetValue (quore);
                        var getter = CountCallCache.Getter (elementType);
                        var count = (long)getter.DynamicInvoke (this, queryable, predicate);
                        var tn = new TypeInfo { Type = elementType }.ImplName;
                        var index = result.GetIndex (tn);
                        if (index != Guid.Empty) {
                            result.Counts[index] = count;
                        }
                    }
                }
            }
            return result;
        }
    }
}
