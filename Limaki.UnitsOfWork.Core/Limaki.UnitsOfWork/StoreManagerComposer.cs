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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common;
using Mono.Linq.Expressions;

namespace Limaki.UnitsOfWork {

    public class StoreManagerComposer<TContainer>
        where TContainer : IMappedContainer {

        protected IEnumerable<(Type type, PropertyInfo cont)> ContainerProperties
            => typeof (TContainer).GetProperties ()
                                  .Where (p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition () == typeof (IEnumerable<>))
                                  .Select (p => ValueTuple.Create (p.PropertyType.GetGenericArguments ()[0], p));

        ILog _log = null;
        public virtual ILog Log {
            get => _log ?? (_log = Registry.Pool.TryGetCreate<Logger> ().Log (this.GetType ()));
            set => _log = value;
        }

        public Action<IStoreManager, Store, Object> CollectEntitiesDelegate () {
            Log.Debug (nameof (CollectEntitiesDelegate));

            var delegateType = typeof (Action<,,>).MakeGenericType (typeof (IStoreManager), typeof (Store), typeof (object));
            var blockExpressions = new List<Expression> ();

            var manager = Expression.Variable (typeof (IStoreManager), "manager");
            var store = Expression.Variable (typeof (Store), "store");
            var item = Expression.Variable (typeof (object), "item");

            var collectMethodGeneric = typeof (IStoreManager).GetMethod (nameof (IStoreManager.CollectEntity)).GetGenericMethodDefinition ();
            foreach (var prop in ContainerProperties) {
                var sinkType = prop.type;
                //           
                var isType = Expression.TypeIs (item, sinkType);
                var collectMethod = collectMethodGeneric.MakeGenericMethod (sinkType);

                var call = Expression.IfThen (isType, Expression.Call (manager, collectMethod, store, Expression.Convert (item, sinkType)));
                blockExpressions.Add (call);
            }

            blockExpressions.Add (Expression.Empty ());

            var composeExpression = Expression.Lambda (delegateType, Expression.Block (blockExpressions), manager, store, item);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Action<IStoreManager, Store, Object>;
        }

        public virtual IEnumerable<T> AddToMap<T> (ListContainer container, IdentityMap map) {
            var items = container.List<T> ();
            //Map.Clear<T>();
            map.RefreshRange (items);
            return items;
        }

        public Action<ListContainer, IdentityMap> ExpandItemsDelegate () {
            Log.Debug (nameof (CollectEntitiesDelegate));

            var delegateType = typeof (Action<,>).MakeGenericType (typeof (ListContainer), typeof (IdentityMap));
            var blockExpressions = new List<Expression> ();

            var thisType = this.GetType ();
            var it = Expression.Variable (thisType, "it");
            var container = Expression.Variable (typeof (ListContainer), "container");
            var map = Expression.Variable (typeof (IdentityMap), "map");

            var addToMapGeneric = thisType.GetMethod (nameof (AddToMap)).GetGenericMethodDefinition ();

            blockExpressions.AddRange (new[]{
                Expression.Assign (it, Expression.Constant (this, thisType)),
            });

            foreach (var prop in ContainerProperties) {
                var sinkType = prop.type;
                //           
                var collectMethod = addToMapGeneric.MakeGenericMethod (sinkType);

                blockExpressions.Add (Expression.Call (it, collectMethod, container, map));
            }

            blockExpressions.Add (Expression.Empty ());

            var composeExpression = Expression.Lambda (delegateType, Expression.Block (new[] { it }, blockExpressions), container, map);
            Log.Debug (composeExpression.ToCSharpCode ());
            var composeAction = composeExpression.Compile ();

            return composeAction as Action<ListContainer, IdentityMap>;
        }
    }
}
