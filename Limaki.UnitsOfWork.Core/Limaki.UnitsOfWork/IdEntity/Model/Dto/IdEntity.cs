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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Limaki.UnitsOfWork.IdEntity.Model.Dto {

    [DataContract]
    public class IdEntity : IIdEntity {

        [DataMember, Key]
        public Guid Id { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime UpdatedAt { get; set; }

    }

}
