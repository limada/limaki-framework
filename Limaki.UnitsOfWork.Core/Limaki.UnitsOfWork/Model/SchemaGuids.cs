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
        /// <summary>
        /// Key = Type.Name
        /// Value = Guid of the Type
        /// </summary>
        public static IDictionary<string, Guid> ModelGuids (this SchemaGuids.IModelGuids model) =>
            model.GetType ().GetProperties (BindingFlags.Static | BindingFlags.Public)
                     .Where (p => p.PropertyType == typeof (Guid))
                      .SelectMany (p => {
                          var guid = (Guid)p.GetValue (null);
                          return p.GetCustomAttributes<TypeGuidAttribute> ().Select (a => a.Type.Name).Select (tg => new { t = tg, g = guid });

                      })
                      .ToDictionary (p => p.t, p => p.g);
    }

}
