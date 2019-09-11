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
using Limaki.UnitsOfWork.Model.Hash;

namespace Limaki.UnitsOfWork.Usecases {

    public class HashVisitor {

        public Action<object> Any { get; set; }

        protected HashSet<string> done = new HashSet<string> ();

        protected bool Visit<T> (T entity, Action<T> visit) where T : IHashEntity {

            if (entity == null || done.Contains (entity.Hash))
                return false;

            Any?.Invoke (entity);
            visit?.Invoke (entity);
            done.Add (entity.Hash);

            return true;

        }

        public void Reset () {
            done.Clear ();
        }
    }
}