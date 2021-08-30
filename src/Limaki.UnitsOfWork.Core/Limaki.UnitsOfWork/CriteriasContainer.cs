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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using MetaLinq;

namespace Limaki.UnitsOfWork {
    
    /// <summary>
    /// Container to hold
    /// serializable Expressions 
    /// used to be sent over Services
    /// </summary>
    [DataContract]
    public class CriteriasContainer<T> where T : Criterias {


        public virtual void ReadFrom(T criteria) {
            var type = criteria.GetType();
            foreach (var prop in type.GetProperties().Where(p => typeof(Expression).IsAssignableFrom(p.PropertyType))) {
                var exp = prop.GetValue(criteria, null) as Expression;
                var thisProp = this.GetType().GetProperty(prop.Name, typeof(EditableExpression));
                if (exp != null && thisProp != null)
                    thisProp.SetValue(this, EditableExpression.CreateEditableExpression(exp), null);

            }
        }

        public virtual void WriteTo (T criteria) {
            var type = criteria.GetType ();
            foreach (var prop in type.GetProperties ().Where (p => typeof (Expression).IsAssignableFrom (p.PropertyType))) {
                var thisProp = this.GetType ().GetProperty (prop.Name, typeof (EditableExpression));
                if (thisProp.GetValue(this) is EditableExpression exp)
                    prop.SetValue (criteria, exp.ToExpression(), null);

            }
        }

    }
}