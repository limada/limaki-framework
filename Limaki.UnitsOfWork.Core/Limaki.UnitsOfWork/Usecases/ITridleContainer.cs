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
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork.Usecases {

    public interface ITridleContainer : IMappedContainer {

        IEnumerable<IStringTridle> StringTridles { get; set; }
        IEnumerable<INumberTridle> NumberTridles { get; set; }
        IEnumerable<IRelationTridle> RelationTridles { get; set; }
        IEnumerable<IByteArrayTridle> ByteArrayTridles { get; set; }
        IEnumerable<IByteArrayTridleMemento> ByteArrayTridleMementos { get; set; }
    }
}
