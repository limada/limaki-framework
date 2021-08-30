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

namespace Limaki.UnitsOfWork.IdEntity.Repository {

    public abstract class EntityIdRepository<TQuore, TCriterias, TCounts, TMapper> : EntityRepository
      where TCounts : EntityIds, new() where TQuore : IEntityQuore where TCriterias : Criterias where TMapper : EntityQuoreMapper, new() {

        public IQueryable<Guid> IdsFor<T> (IQueryable<T> query, Expression<Func<T, bool>> where) where T : IIdEntity => query.Where (where).Select (i => i.Id);

        protected EntityCallCache IdCountCallCache => new EntityCallCache (ExpressionUtils.Lambda
                                                                           <Func<EntityIdRepository<TQuore, TCriterias, TCounts, TMapper>,
                                                                           IQueryable<Model.Dto.IdEntity>, Expression<Func<Model.Dto.IdEntity, bool>>, IQueryable<Guid>>>
                                                             ((counter, querable, predicate) => counter.IdsFor (querable, predicate)));

        EntityQuoreMapper Mapper = new TMapper ();


        public TCounts IdsFor (TCriterias preds, TQuore quore) {
            Log.Debug (nameof (IdsFor));
            var result = new TCounts ();

            foreach (var predicateProp in preds.GetType ().GetProperties ().Where (p => p.PropertyType.BaseType == typeof (LambdaExpression))) {
                if (predicateProp.GetValue (preds) is LambdaExpression predicate) {
                    Log.Debug (predicate.ToString ());
                    var elementType = predicate.Type.GenericTypeArguments[0];
                    var implType = Mapper.MapIn (predicate.Type.GenericTypeArguments[0]);
                    predicate = Mapper.Map (predicate, implType) as LambdaExpression;
                    if (quore.QueryableOf (implType) is IQueryable queryable) {
                        var getter = IdCountCallCache.Getter (implType);
                        var ids = (IQueryable<Guid>)getter.DynamicInvoke (this, queryable, predicate);
                        var index = result.GetIndex (implType);
                        if (index != Guid.Empty) {
                            result.Ids[index] = ids.ToArray ();
                        }
                    }
                }
            }
            return result;
        }
    }
}
