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

namespace Limaki.UnitsOfWork.IdEntity.Repository {

    public abstract class EntityRepositoryCounter<TQuore, TCriterias, TCounts, TMapper> : EntityRepository
      where TCounts : EntityCounts, new() where TQuore : IEntityQuore where TCriterias : Criterias where TMapper : EntityQuoreMapper, new() {

        public long Count<T> (IQueryable<T> query, Expression<Func<T, bool>> where) => query.Where (where).LongCount ();

        protected CallCache CountCallCache => new CallCache (ExpressionUtils.Lambda
          <Func<EntityRepositoryCounter<TQuore, TCriterias, TCounts, TMapper>, IQueryable<CallCache.Entity>, Expression<Func<CallCache.Entity, bool>>, long>>
             ((counter, querable, predicate) => counter.Count (querable, predicate)));

        EntityQuoreMapper Mapper = new TMapper ();

        public TCounts Count (TCriterias criterias, TQuore quore) {
            Log.Debug (nameof (Count));
            var result = new TCounts ();

            foreach (var criteriaProp in criterias.GetType ().GetProperties ().Where (p => p.PropertyType.BaseType == typeof (LambdaExpression))) {
                if (criteriaProp.GetValue (criterias) is LambdaExpression criteria) {
                    Log.Debug (criteria.ToString ());
                    var elementType = Mapper.MapIn (criteria.Type.GenericTypeArguments [0]);
                    criteria = Mapper.Map (criteria, elementType) as LambdaExpression;
                    var queryableProperty = quore.GetType ()
                        .GetProperties ()
                        .FirstOrDefault (p => p.PropertyType.IsGenericType
                                              && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>)
                                              && p.PropertyType.GenericTypeArguments [0] == elementType);
                    if (queryableProperty != null) {
                        var queryable = queryableProperty.GetValue (quore);
                        var getter = CountCallCache.Getter (elementType);
                        var count = (long)getter.DynamicInvoke (this, queryable, criteria);
                        var index = result.GetIndex (elementType);
                        if (index != Guid.Empty) {
                            result.Counts [index] = count;
                        }
                    }
                }
            }
            return result;
        }
    }
}
