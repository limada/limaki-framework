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
using System.Runtime.Serialization;
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.Tridles.Model {

    public interface IByteArrayTridleMemento : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Member { get; set; }

    }
}

