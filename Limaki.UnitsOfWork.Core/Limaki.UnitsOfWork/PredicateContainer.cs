/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using MetaLinq;

namespace Limaki.Common.UnitsOfWork {
    /// <summary>
    /// Container to hold
    /// serializable Expressions 
    /// used to be sent over WCF-Services
    /// </summary>
    [DataContract]
    public class PredicateContainer<T> where T : QueryPredicates {
        public virtual void ReadFrom(T predicates) {
            var type = predicates.GetType();
            foreach (var prop in type.GetProperties().Where(p => typeof(Expression).IsAssignableFrom(p.PropertyType))) {
                var exp = prop.GetValue(predicates, null) as Expression;
                var thisProp = this.GetType().GetProperty(prop.Name, typeof(EditableExpression));
                if (exp != null && thisProp != null)
                    thisProp.SetValue(this, EditableExpression.CreateEditableExpression(exp), null);

            }
        }
    }
}