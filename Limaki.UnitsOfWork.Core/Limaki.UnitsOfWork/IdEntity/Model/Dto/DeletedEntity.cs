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

namespace Limaki.UnitsOfWork.IdEntity.Model.Dto {

    public class DeletedEntity : IdEntity, IDeletedEntity {

        [DataMember]
        public Guid ModelType { get; set; }

        [DataMember]
        public DateTime DeletedAt { get; set; }
    }

}
