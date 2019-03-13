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

namespace Limaki.UnitsOfWork.Usecases {

    public class EntityVisitor {
        
        public Action<object> Any { get; set; }

        protected HashSet<Guid> done = new HashSet<Guid> ();

        protected bool Visit<T> (T entity, Action<T> visit) where T : IIdEntity {

            if (entity == null || done.Contains (entity.Id))
                return false;

            Any?.Invoke (entity);
            visit?.Invoke (entity);
            done.Add (entity.Id);

            return true;

        }

        public void Reset () {
            done.Clear ();
        }
    }

}
