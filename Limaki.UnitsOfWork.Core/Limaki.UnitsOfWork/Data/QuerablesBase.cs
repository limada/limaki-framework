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
using System.IO;
using System.Linq;
using System.Reflection;
using Limaki.Common.Collections;
using Mono.Linq.Expressions;

namespace Limaki.UnitsOfWork.Data {

    public class QuerablesBase {

        public Paging Paging { get; set; }

        public virtual Guid MainQuery { get; set; }
        public virtual GuidFlags Resolve { get; set; }

        public string Info () {
            var writer = new StringWriter ();
            writer.WriteLine (this.GetType ().Name);
            foreach (var p in QueryableProperties ()) {
                if (p.GetValue (this, null) is IQueryable querable) {
                    var expString = querable.Expression.ToCSharpCode ().Replace ('\n', ' ').Replace ('\t', ' ').Replace ("  ", " ");
                    writer.WriteLine ($"\t{nameof (IQueryable)}<{p.PropertyType.GetGenericArguments ().First ().Name}> => {expString}");
                }
            }
            return writer.ToString();
        }

        protected virtual IEnumerable<PropertyInfo> QueryableProperties () {
            return this.GetType ().GetProperties ()
               .Where (p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>));
        }
    }
}
