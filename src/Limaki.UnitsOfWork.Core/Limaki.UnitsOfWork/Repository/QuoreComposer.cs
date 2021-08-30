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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Repository;
using Limaki.UnitsOfWork.IdEntity.Repository;
using Limaki.UnitsOfWork.IdEntity.Model;
using Mono.Linq.Expressions;

namespace Limaki.UnitsOfWork.Repository {
    
    public class QuoreComposer<TQuore, TMapper> : QuoreComposerBase<TQuore, TMapper>
        where TQuore : IEntityQuore
        where TMapper : EntityQuoreMapper, new() {

        protected Action<IQuore, IEnumerable> UpsertRemoveDelegate (string method) {

            var delegateType = typeof (Action<,>).MakeGenericType (typeof (IQuore), typeof (IEnumerable));
            var blockExpressions = new List<Expression> ();

            var quore = Expression.Variable (typeof (IQuore), "quore");
            var entities = Expression.Variable (typeof (IEnumerable), "entities");

            var ofTypeGeneric = typeof (Enumerable).GetMethod (nameof (Enumerable.OfType));
            var quoreCallGeneric = typeof (IQuore).GetMethods()
                .FirstOrDefault(m => m.Name == method && m.GetParameters()
                    .FirstOrDefault(para=>para.ParameterType.GetGenericTypeDefinition ()==typeof(IEnumerable<>))!=null);
            foreach (var prop in QuoreProperties) {
                var sinkType = prop.type;
                var sourceType = Mapper.MapOut (prop.type);

                if (sourceType != null) {
                    // Quore.Upsert (entities.OfType<IBusinessEntity> ());
                    var offType = Expression.Call (null, ofTypeGeneric.MakeGenericMethod (sourceType), entities);
                    var call = Expression.Call (quore, quoreCallGeneric.MakeGenericMethod (sourceType), offType);

                    if (false) {
                        var entitiesType = typeof (IEnumerable<>).MakeGenericType (sourceType);
                        var isType = Expression.TypeIs (entities, entitiesType);
                        var asType = Expression.TypeAs (entities, entitiesType);
                    }
                    blockExpressions.Add (call);
                }

            }
            blockExpressions.Add (Expression.Empty ());
            var composeExpression = Expression.Lambda (delegateType, Expression.Block (blockExpressions), quore, entities);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Action<IQuore, IEnumerable>;
        }

        public Action<IQuore, IEnumerable> UpsertDelegate () {
            Log.Debug (nameof (UpsertDelegate));

            return UpsertRemoveDelegate (nameof (IQuore.Upsert));

        }

        public Action<IQuore, IEnumerable> RemoveDelegate () {
            Log.Debug (nameof (RemoveDelegate));

            return UpsertRemoveDelegate (nameof (IQuore.Remove));

        }

        // void Remove (IEnumerable<Guid> ids)
        public Action<IQuore, Guid> RemoveIdDelegate () { 
            Log.Debug (nameof (RemoveIdDelegate));

            var delegateType = typeof (Action<,>).MakeGenericType (typeof (IQuore), typeof (Guid));
            var blockExpressions = new List<Expression> ();

            var quore = Expression.Variable (typeof (IQuore), "quore");
            var id = Expression.Variable (typeof (Guid), "id");
            var method = nameof (IQuore.Remove);
            var quoreCallGeneric = typeof (IQuore).GetMethods ()
                .FirstOrDefault (m => m.Name == method && m.GetParameters ()
                    .FirstOrDefault (para => para.ParameterType.GetGenericTypeDefinition ().BaseType == typeof (LambdaExpression)) != null);
            var entIdMeth = typeof (IIdEntity).GetMember (nameof (IIdEntity.Id))[0];
            foreach (var prop in QuoreProperties) {
                var sinkType = prop.type;
                var sourceType = Mapper.MapOut (prop.type);

                if (sourceType != null) {
                    var ent = Expression.Variable (sourceType, "e");
                    var entId = Expression.MakeMemberAccess (ent, entIdMeth);
                    var where = Expression.Lambda (Expression.Equal (entId, id), ent);

                    var call = Expression.Call (quore, quoreCallGeneric.MakeGenericMethod (sourceType), where);

                    blockExpressions.Add (call);
                }

            }
            blockExpressions.Add (Expression.Empty ());
            var composeExpression = Expression.Lambda (delegateType, Expression.Block (blockExpressions), quore, id);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Action<IQuore, Guid>;
        }

    }
}
