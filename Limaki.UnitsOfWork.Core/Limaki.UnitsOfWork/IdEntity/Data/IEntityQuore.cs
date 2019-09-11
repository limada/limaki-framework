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
using Limaki.LinqData;
using Limaki.UnitsOfWork.IdEntity.Model;
using Limaki.UnitsOfWork.IdEntity.Model.Dto;

namespace Limaki.UnitsOfWork.IdEntity.Data {

    public interface IEntityQuore : IDomainQuore {

        IQueryable<DeletedEntity> DeletedEntitys { get; }

        void Upsert<T> (IEnumerable<T> entities) where T : IIdEntity;

        void Remove<T> (IEnumerable<T> entities) where T : IIdEntity;
    }
}
