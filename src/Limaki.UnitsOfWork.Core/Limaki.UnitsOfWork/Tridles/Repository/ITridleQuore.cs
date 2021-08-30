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

using System.Linq;
using Limaki.UnitsOfWork.Tridles.Model.Dto;

namespace Limaki.UnitsOfWork.Tridles.Repository {

    public interface ITridleQuore {

        IQueryable<StringTridle> StringTridles { get; }

        IQueryable<NumberTridle> NumberTridles { get; }

        IQueryable<RelationTridle> RelationTridles { get; }

        IQueryable<ByteArrayTridle> ByteArrayTridles { get; }

        IQueryable<ByteArrayTridleMemento> ByteArrayTridleMementos { get; }

    }
}
