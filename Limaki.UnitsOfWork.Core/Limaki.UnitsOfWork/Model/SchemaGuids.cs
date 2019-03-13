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
using System.Reflection;

namespace Limaki.UnitsOfWork.Model {

    public class SchemaGuids {
        public interface IModelGuids {
        }
    }

    public static class SchemaExtensions {
        public static IDictionary<string, Guid> ModelGuids (this SchemaGuids.IModelGuids model) =>
        model.GetType ().GetProperties (BindingFlags.Static | BindingFlags.Public)
                 .Where (p => p.PropertyType == typeof (Guid))
                  .Select (p => new { t = p.Name, g = (Guid)p.GetValue (null) })
                  .ToDictionary (p => p.t, p => p.g);
    }

}
