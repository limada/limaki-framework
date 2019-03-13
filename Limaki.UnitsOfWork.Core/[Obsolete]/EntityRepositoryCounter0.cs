/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
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

	[Obsolete]
	public abstract class EntityRepositoryCounter0<Q, P, C, M> : EntityRepository 
        where C : EntityCounts0, new() where Q : IEntityQuore where P : QueryPredicates where M : EntityQuoreMapper, new() {

        public long Count<T> (IQueryable<T> query, Expression<Func<T, bool>> where) => query.Where (where).LongCount ();

        protected CallCache CountCallCache => new CallCache (ExpressionUtils.Lambda
                <Func<EntityRepositoryCounter0<Q, P, C, M>, IQueryable<CallCache.Entity>, Expression<Func<CallCache.Entity, bool>>, long>>
                 ((l, q, e) => l.Count (q, e)));

        EntityQuoreMapper Mapper = new M ();

        public C Count (P preds, Q quore) {
            Log.Debug (nameof (Count));
            var result = new C ();

            foreach (var whereProps in preds.GetType ().GetProperties ().Where (p => p.PropertyType.BaseType == typeof (LambdaExpression))) {
                var where = whereProps.GetValue (preds) as LambdaExpression;
                if (where != null) {
                    Log.Debug (where.ToString ());
                    var t = Mapper.MapIn (where.Type.GenericTypeArguments[0]);
                    where = Mapper.Map (where, t) as LambdaExpression;
                    var queryableProperty = quore.GetType ().GetProperties ()
                                 .Where (p => p.PropertyType.IsGenericType
                                         && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>)
                                         && p.PropertyType.GenericTypeArguments[0] == t)
                                 .FirstOrDefault ();
                    if (queryableProperty != null) {
                        var queryable = queryableProperty.GetValue (quore);
                        var getter = CountCallCache.Getter (t);
                        var count = (long)getter.DynamicInvoke (this, queryable, where);
                        var tn = new TypeInfo { Type = t }.ImplName;
                        var index = result.GetIndex (tn);
                        if (index != -1) {
                            result.Counts[index] = count;
                        }
                    }
                }
            }
            return result;
        }
    }
}
