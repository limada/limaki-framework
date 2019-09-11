/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.Common;
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.IdEntity {

    public class IdEntityStore<F> : Store<F> where F : IFactory, new() {

        public override T Update<T> (T item) {
            item = base.Update (item);
            if (item is IIdEntity idEntity)
                idEntity.UpdatedAt = DateTime.Now;
            return item;
        }
    }

}