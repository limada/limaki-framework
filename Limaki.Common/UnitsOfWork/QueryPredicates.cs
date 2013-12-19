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

using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Mono.Linq.Expressions;

namespace Limaki.Common.UnitsOfWork {

    public class QueryPredicates {

        public Paging Paging { get; set; }

        public string ToCSharpCode() {
            var type = this.GetType();
            using (var writer = new StringWriter()) {
                foreach (var prop in type.GetProperties().Where(p => typeof(Expression).IsAssignableFrom(p.PropertyType))) {
                    var exp = prop.GetValue(this, null) as Expression;
                    if (exp != null)
                        writer.WriteLine(exp.ToCSharpCode().Replace("\r\n", " ").Replace('\t', ' ').Replace("  ", " "));
                }
                return writer.ToString();
            }
        }

    }
}