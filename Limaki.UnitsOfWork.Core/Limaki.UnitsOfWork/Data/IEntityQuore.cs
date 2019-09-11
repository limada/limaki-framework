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

using System.Collections.Generic;
using System.Linq;
using Limaki.Data;
using Limaki.LinqData;
using Limaki.UnitsOfWork.Model;
using Limaki.UnitsOfWork.Model.Dto;

namespace Limaki.UnitsOfWork.Data {

    public interface IEntityQuore : IDomainQuore {

        IQueryable<DeletedEntity> DeletedEntitys { get; }

        void Upsert<T> (IEnumerable<T> entities) where T : IIdEntity;

        void Remove<T> (IEnumerable<T> entities) where T : IIdEntity;
    }
}
