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
using System.Linq.Expressions;
using Limaki.Common.Linqish;
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork.Data {

    public class EntityCallCache {

        public EntityCallCache (Expression call) { this.Call = call; }

        public Expression Call { get; protected set; }

        private Dictionary<Type, Delegate> _cache = new Dictionary<Type, Delegate> ();

        public Delegate Getter (Type cType) {
            Delegate getter = null;
            if (!_cache.TryGetValue (cType, out getter)) {
                var changer = new ExpressionChangerVisit (typeof (IdEntity), cType);
                var expr = changer.Visit (Call);
                getter = (expr as LambdaExpression).Compile ();

                _cache.Add (cType, getter);
            }
            return getter;
        }
    }
}
