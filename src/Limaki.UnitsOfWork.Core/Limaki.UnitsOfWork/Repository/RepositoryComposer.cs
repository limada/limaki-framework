/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.UnitsOfWork.IdEntity.Repository;
using Mono.Linq.Expressions;

namespace Limaki.UnitsOfWork.Repository {

    public class RepositoryComposer<TQuore, TChangesetContainer, TMapper> : QuoreComposerBase<TQuore, TMapper>
        where TMapper : EntityQuoreMapper, new()
        where TQuore : IEntityQuore
        where TChangesetContainer : ChangeSetContainer {

        protected IEnumerable<(Type type, PropertyInfo cont, PropertyInfo ore)> ContainerProperties {
            get {
                var container = typeof (TChangesetContainer).GetProperties ().Where (p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition () == typeof (ChangeSet<>))
                    .Select (p => new { type = Mapper.MapIn (p.PropertyType.GetGenericArguments ()[0]), p });

                return container.Join (QuoreProperties, cont => cont.type, ore => ore.type, (cont, ore) => new { type = cont.type, cont = cont.p, ore = ore.ore })
                         .Select (prop => ValueTuple.Create (prop.type, prop.cont, prop.ore));

            }
        }

        protected Func<EntityRepository, TQuore, TChangesetContainer, Guid, int> RemoveUpsertDelegate (string repositoryCall, string changesetProperty, IEnumerable<(Type type, PropertyInfo cont, PropertyInfo ore)> containerProperties) {

            var delegateType = typeof (Func<,,,,>).MakeGenericType (typeof (EntityRepository), typeof (TQuore), typeof (TChangesetContainer), typeof (Guid), typeof (int));
            var blockExpressions = new List<Expression> ();

            var repository = Expression.Variable (typeof (EntityRepository), "repository");
            var container = Expression.Variable (typeof (TChangesetContainer), "container");
            var quore = Expression.Variable (typeof (TQuore), "quore");
            var userId = Expression.Variable (typeof (Guid), "userId");
            var count = Expression.Variable (typeof (int), "count");

            var upsertMethGeneric = typeof (EntityRepository).GetMethod (repositoryCall).GetGenericMethodDefinition ();

            blockExpressions.AddRange (new[] {
                Expression.Assign(count, Expression.Constant(0)),
            });

            foreach (var prop in containerProperties) {
                var sinkType = prop.type;
                var sourceType = prop.cont.PropertyType.GetGenericArguments ()[0];

                var quoreQueryable = Expression.Property (quore, prop.ore.GetMethod);
                var changeSet = Expression.Property (container, prop.cont.GetMethod);
                var containerEntities = Expression.MakeMemberAccess (changeSet, prop.cont.PropertyType.GetProperty (changesetProperty));

                var upsertMeth = upsertMethGeneric.MakeGenericMethod (typeof (TQuore), sourceType);
                var upsertCall = Expression.AddAssign (count, Expression.Call (repository, upsertMeth, quore, containerEntities, userId));
                var ifNotNull = Expression.NotEqual (Expression.Default (changeSet.Type), changeSet);

                blockExpressions.Add (Expression.IfThen (ifNotNull, upsertCall));
            }

            blockExpressions.Add (count);
            var composeExpression = Expression.Lambda (delegateType, Expression.Block (new[] { count }, blockExpressions), repository, quore, container, userId);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Func<EntityRepository, TQuore, TChangesetContainer, Guid, int>;
        }

        public virtual Func<EntityRepository, TQuore, TChangesetContainer, Guid, int> UpsertDelegate () {

            Log.Debug (nameof (UpsertDelegate));
            return RemoveUpsertDelegate (nameof (EntityRepository.UpsertEntities), nameof (ChangeSet<object>.Updated), ContainerProperties);

        }

        public virtual Func<EntityRepository, TQuore, TChangesetContainer, Guid, int> RemoveDelegate () {

            Log.Debug (nameof (RemoveDelegate));
            return RemoveUpsertDelegate (nameof (EntityRepository.RemoveEntities), nameof (ChangeSet<object>.Removed), ContainerProperties);

        }
    }
}
