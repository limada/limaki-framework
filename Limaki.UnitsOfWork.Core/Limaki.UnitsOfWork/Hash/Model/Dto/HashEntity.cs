﻿/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Limaki.UnitsOfWork.Hash.Model.Dto {

    public class HashEntity : IHashEntity {

        [DataMember, Key]
        public string Hash { get; set; }

        //[DataMember]
        //public DateTime CreatedAt { get; set; }

        //[DataMember]
        //public DateTime UpdatedAt { get; set; }
    }


}
