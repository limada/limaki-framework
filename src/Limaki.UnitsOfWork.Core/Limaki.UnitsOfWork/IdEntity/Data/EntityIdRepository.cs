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
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.IdEntity.Data {

    public abstract class EntityIdRepository<TQuore, TPredicates, TCounts, TMapper> : EntityRepository
      where TCounts : EntityIds, new() where TQuore : IEntityQuore where TPredicates : QueryPredicates where TMapper : EntityQuoreMapper, new() {

        public IQueryable<Guid> IdsFor<T> (IQueryable<T> query, Expression<Func<T, bool>> where) where T : IIdEntity => query.Where (where).Select (i => i.Id);

        protected EntityCallCache IdCountCallCache => new EntityCallCache (ExpressionUtils.Lambda
                                                                           <Func<EntityIdRepository<TQuore, TPredicates, TCounts, TMapper>,
                                                                           IQueryable<Model.Dto.IdEntity>, Expression<Func<Model.Dto.IdEntity, bool>>, IQueryable<Guid>>>
                                                             ((counter, querable, predicate) => counter.IdsFor (querable, predicate)));

        EntityQuoreMapper Mapper = new TMapper ();


        public TCounts IdsFor (TPredicates preds, TQuore quore) {
            Log.Debug (nameof (IdsFor));
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
                        var getter = IdCountCallCache.Getter (elementType);
                        var count = (IQueryable<Guid>)getter.DynamicInvoke (this, queryable, predicate);
                        var tn = new TypeInfo { Type = elementType }.ImplName;
                        var index = result.GetIndex (tn);
                        if (index != Guid.Empty) {
                            result.Ids[index] = count.ToArray ();
                        }
                    }
                }
            }
            return result;
        }
    }
}
