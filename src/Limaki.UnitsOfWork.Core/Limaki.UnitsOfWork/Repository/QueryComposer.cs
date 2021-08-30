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
using Limaki.Common.Collections;
using Limaki.UnitsOfWork.IdEntity.Repository;
using Mono.Linq.Expressions;

namespace Limaki.UnitsOfWork.Repository {

    public class QueryComposer<TQuore, TCriterias, TQuerables, TMapper> : QuoreComposerBase<TQuore,TMapper>
        where TQuerables : QuerablesBase, new() 
        where TQuore : IEntityQuore
        where TCriterias : ICriterias
        where TMapper : EntityQuoreMapper, new() 
       {
        
        protected IEnumerable<(Type type, PropertyInfo ry, PropertyInfo pre, PropertyInfo ore)> RepositoryProperties {
            get {
                var query = typeof (TQuerables).GetProperties ().Where (p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>))
                      .Select (p => new { type = p.PropertyType.GetGenericArguments ()[0], p });

                var criterias = typeof (TCriterias).GetProperties ().Where (p => p.PropertyType.BaseType == typeof (LambdaExpression))
                          .Select (p => new { type = Mapper.MapIn (p.PropertyType.GetGenericArguments ()[0].GetGenericArguments ()[0]), p });
                
                return query.Join (criterias, ry => ry.type, pre => pre.type, (ry, pre) => new { type = ry.type, ry = ry.p, pre = pre.p })
                            .Join (QuoreProperties, join => join.type, ore => ore.type, (join, ore) => new { type = join.type, ry = join.ry, pre = join.pre, ore = ore.ore })
                                 .Select (prop => ValueTuple.Create (prop.type, prop.ry, prop.pre, prop.ore));

            }
        }

        public Func<TQuore, TCriterias, TQuerables> ComposeQuerablesDelegate () {
            Log.Debug (nameof (ComposeQuerablesDelegate));

            var delegateType = typeof (Func<,,>).MakeGenericType (typeof (TQuore), typeof (TCriterias), typeof (TQuerables));

            var blockExpressions = new List<Expression> ();
            var repository = Expression.Variable (typeof (EntityRepository), "repository");
            var criterias = Expression.Variable (typeof (TCriterias), "criterias");
            var quore = Expression.Variable (typeof (TQuore), "quore");
            var query = Expression.Variable (typeof (TQuerables), "query");

            var convertMethGeneric = typeof (EntityRepository).GetMethod (nameof (EntityRepository.WhereIfConvertF)).GetGenericMethodDefinition ();

            var queryResolve = Expression.Property (query, typeof (TQuerables).GetProperty (nameof (Criterias.Resolve), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
            var queryMain = Expression.Property (query, typeof (TQuerables).GetProperty (nameof (Criterias.MainQuery)));

            var criteriasResolve = Expression.Property (criterias, typeof (TCriterias).GetProperty (nameof (Criterias.Resolve), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
            var criteriasMain = Expression.Property (criterias, typeof (TCriterias).GetProperty (nameof (Criterias.MainQuery)));

            blockExpressions.AddRange (new[]{
                Expression.Assign (repository, Expression.Constant (this, typeof (EntityRepository))),
                Expression.Assign (query, Expression.New (typeof (TQuerables))),
                Expression.Assign (queryResolve, criteriasResolve),
                Expression.Assign (queryMain, criteriasMain),
            });

            foreach (var prop in RepositoryProperties) {

                var sinkType = prop.type;
                var sourceType = prop.pre.PropertyType.GetGenericArguments ()[0].GetGenericArguments ()[0];
                var convertMeth = convertMethGeneric.MakeGenericMethod (sourceType, sinkType);

                var queryQueryable = Expression.Property (query, prop.ry.GetMethod);
                var quoreQueryable = Expression.Property (quore, prop.ore.GetMethod);
                var criteriasExpression = Expression.Property (criterias, prop.pre.GetMethod);
                var convertExpression = Expression.Call (repository, convertMeth, quoreQueryable, criteriasExpression);
                var setExpression = Expression.Assign (queryQueryable, convertExpression);
                blockExpressions.Add (setExpression);
            }

            blockExpressions.Add (query);

            var composeExpression = Expression.Lambda (delegateType, Expression.Block (new[] { repository, query }, blockExpressions), quore, criterias);
            var composeAction = composeExpression.Compile ();

            Log.Debug (composeExpression.ToCSharpCode ());
            return composeAction as Func<TQuore, TCriterias, TQuerables>;
        }

        // public virtual void AddToMap (IdentityMap map, Querables query) {...}
        public Action<IdentityMap, TQuerables> AddToMapDelegate () {

            Log.Debug (nameof (AddToMapDelegate));

            var delegateType = typeof (Action<,>).MakeGenericType (typeof (IdentityMap), typeof (TQuerables));
            var blockExpressions = new List<Expression> ();

            var repository = Expression.Variable (typeof (EntityRepository), "repository");
            var query = Expression.Variable (typeof (TQuerables), "query");
            var map = Expression.Variable (typeof (IdentityMap), "map");

            var flag = Expression.Variable (typeof (Guid), "flag");

            var queryResolve = Expression.Property (query, typeof (TQuerables).GetProperty (nameof (Criterias.Resolve), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));


            blockExpressions.Add (Expression.Assign (repository, Expression.Constant (this, typeof (EntityRepository))));
            foreach (var prop in RepositoryProperties) {
                /*
                 var flag = query.Resolve.FlagOf (BusinessEntityFlag);
                 if (query.Resolve.HasFlag (TradeRelations.Product)) {
                    AddToMap<IProduct> (map, query.Products.Distinct ());
                }

                */
                var sourceType = prop.pre.PropertyType.GetGenericArguments ()[0].GetGenericArguments ()[0];
                var sinkType = prop.type;

                // TODO: make use of TypeGuidAttribute
                var flagOf = Expression.Call (queryResolve, typeof (GuidFlags).GetMethod (nameof (GuidFlags.FlagOf), new Type[] { typeof (Type) }), Expression.Constant (prop.type));
                var assignFlag = Expression.Assign (flag, flagOf);
                blockExpressions.Add (assignFlag);

                var hasFlag = Expression.Call (queryResolve, typeof (GuidFlags).GetMethod (nameof (GuidFlags.HasFlag), new Type[] { typeof (Guid) }), flag);

                var queryQueryable = Expression.Property (query, prop.ry.GetMethod);
                var callMethod = typeof (EntityRepository).GetMethod (nameof (EntityRepository.AddToMapNoChecks)).MakeGenericMethod (sourceType);

                var addToMap = Expression.Call (repository, callMethod, map, queryQueryable);
                blockExpressions.Add (Expression.IfThen (hasFlag, addToMap));

            }
            blockExpressions.Add (Expression.Empty ());
            var composeExpression = Expression.Lambda (delegateType, Expression.Block (new[] { repository, flag }, blockExpressions), map, query);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Action<IdentityMap, TQuerables>;
        }

        /*
         public virtual void FillContainer (ListContainer result, IdentityMap map) {
            result.Set (map.Stored<IProduct> ()?.ToArray ());
            ...
        }
         */

        public Action<IMappedContainer, IdentityMap> FillContainerDelegate () {
            Log.Debug (nameof (FillContainerDelegate));

            var delegateType = typeof (Action<,>).MakeGenericType (typeof (IMappedContainer), typeof (IdentityMap));
            var blockExpressions = new List<Expression> ();

            var container = Expression.Variable (typeof (IMappedContainer), "container");
            var map = Expression.Variable (typeof (IdentityMap), "map");

            foreach (var prop in RepositoryProperties) {

                var sourceType = prop.pre.PropertyType.GetGenericArguments ()[0].GetGenericArguments ()[0];

                var storedType = typeof (IEnumerable<>).GetGenericTypeDefinition ().MakeGenericType (sourceType);
                var setMethod = typeof (IListContainer).GetMethod (nameof (IListContainer.Set)).MakeGenericMethod (sourceType);
                var storedMethod = typeof (IdentityMap).GetMethod (nameof (IdentityMap.Stored)).MakeGenericMethod (sourceType);

                var stored = Expression.Variable (storedType, "stored");
                var storedExpression = Expression.Call (map, storedMethod);

                var toArray = Expression.Call (null, typeof (Enumerable).GetMethod (nameof (Enumerable.ToArray)).GetGenericMethodDefinition ().MakeGenericMethod (sourceType), stored);
                var ifNull = Expression.NotEqual (Expression.Default (storedType), stored);
                var setExpression = Expression.Call (container, setMethod, stored);

                blockExpressions.Add (Expression.Block (new[] { stored }, Expression.Assign (stored, storedExpression), Expression.IfThen (ifNull, setExpression)));
            }

            blockExpressions.Add (Expression.Empty ());
            var composeExpression = Expression.Lambda (delegateType, Expression.Block (blockExpressions), container, map);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Action<IMappedContainer, IdentityMap>;
        }

    }
}
