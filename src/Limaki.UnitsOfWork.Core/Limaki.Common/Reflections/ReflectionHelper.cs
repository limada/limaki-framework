using System;
using System.Collections.Generic;
using System.Linq;

namespace Limaki.Common.Reflections {
    
    public class ReflectionHelper {
        
        public IEnumerable<TypeInfo> GetTypeInfos (IEnumerable<Type> types) {
            return types.Select (t => new TypeInfo { Type = t });
        }

        public IEnumerable<TypeInfo> GetTypeInfosFromNameSpace (string nspace) {
            var q = AppDomain.CurrentDomain.GetAssemblies ()
                       .SelectMany (t => t.GetTypes ())
                       .Where (t => t.Namespace == nspace);
            return GetTypeInfos (q);
        }
    }
}
