/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2016-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Limaki.Common.Reflections {
    
    public class ReflectionHelper {
        
        public IEnumerable<TypeInfoEx> GetTypeInfos (IEnumerable<Type> types) {
            return types.Select (t => new TypeInfoEx { Type = t });
        }

        public IEnumerable<TypeInfoEx> GetTypeInfosFromNameSpace (string nspace) {
            var q = AppDomain.CurrentDomain.GetAssemblies ()
                       .SelectMany (t => t.GetTypes ())
                       .Where (t => t.Namespace == nspace);
            return GetTypeInfos (q);
        }
    }
}
