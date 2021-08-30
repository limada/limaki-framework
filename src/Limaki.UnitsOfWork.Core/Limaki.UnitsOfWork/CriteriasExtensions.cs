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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Limaki.UnitsOfWork {

    public static class CriteriasExtensions {

        public static (LambdaExpression expression, Type matchType) ExpressionOf<E> (this ICriterias criterias) => criterias.ExpressionOf (typeof (E));

        /// <summary>
        /// matching Expression the of <see cref="Criterias"/>
        /// e can be an implementation of interface I + e.Name
        /// </summary>
        /// <returns>matching Expression, matching Type</returns>
        public static (LambdaExpression expression, Type matchType) ExpressionOf (this ICriterias criterias, Type e) {

            // TODO DOING: get rid of TypeInfoEx - naming-conventions
            //var entityType = new Common.Reflections.TypeInfoEx { Type = e };
            //var entityPred = preds.GetType ().GetProperties ().First (p => p.Name == entityType.GetPlural (entityType.ImplName)).GetValue (preds);


            var matchTypes = new Queue<Type> ();
            matchTypes.Enqueue (e);
            foreach (var intfType in e.GetInterfaces ().Where (i => i.Name == $"I{e.Name}")) {
                matchTypes.Enqueue (intfType);
            }
            while (matchTypes.Count != 0) {
                var matchType = matchTypes.Dequeue ();
                var fT = typeof (Func<,>).MakeGenericType (matchType, typeof (bool));
                var eT = typeof (Expression<>).MakeGenericType (fT);
                if (criterias.GetType ().GetProperties ().FirstOrDefault (p => p.PropertyType == eT) is System.Reflection.PropertyInfo prop) {
                    return (prop.GetValue (criterias) as LambdaExpression, matchType);
                }
            }

            return default;
        }
    }

}
