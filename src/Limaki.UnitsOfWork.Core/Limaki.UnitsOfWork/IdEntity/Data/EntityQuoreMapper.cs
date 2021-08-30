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
using Limaki.Common.Linqish;
using Limaki.LinqData;
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.IdEntity.Data {
    
    public class EntityQuoreMapper : IQuoreMapper {

        #region Expression-Conversion

        ExpressionCache _expressionCache = null;
        public ExpressionCache ExpressionCache {
            get { return _expressionCache ?? (_expressionCache = new ExpressionCache ()); }
            set { _expressionCache = value; }
        }

        protected Expression ChangeExpr<T> (Expression expr) {
            var type = typeof (T);
            return ExpressionChangerVisit.Change (expr, type, MapIn (type));
        }

        /// <summary>
        /// this is only needed if
        /// InnerContext does not support local constants or
        /// InnerContext does not support interface lamda expressions on entities
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual Expression Map (Expression predicate, Type elementType) {

            var expr = EvaluatingExpressionVisitor.Evaluate (predicate);
            if (ExpressionCache.TryGet (ref expr)) {
                return expr;
            }

            foreach (var map in Factory.EntityMapping) {
                expr = ExpressionChangerVisit.Change (expr, map.Item1, map.Item2);
            }

            ExpressionCache.Add (expr);

            return expr;

        }

        #endregion

        protected EntityFactory Factory { get; set; }

        public virtual Type MapIn (Type baseType) {

            var m = Factory.EntityMapping.FirstOrDefault (e => e.Item1 == baseType);
            if (m == null)
                return null;

            return m.Item2;

        }

        public virtual Type MapOut (Type sinkType) {

            var m = Factory.EntityMapping.FirstOrDefault (e => e.Item2 == sinkType);
            if (m == null)
                return null;

            return m.Item1;

        }

        public virtual IEnumerable<T> MapIn<T> (IEnumerable<T> entities) {
            return entities;
        }

        public virtual IQueryable<T> MapQuery<T> (IQuore quore) {

            return null;

        }
    }

}
