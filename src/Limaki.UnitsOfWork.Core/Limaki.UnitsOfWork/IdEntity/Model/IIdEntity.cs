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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Limaki.UnitsOfWork.IdEntity.Model {

    public interface IIdEntity {

        [DataMember, Key]
        Guid Id { get; set; }

        [DataMember]
        DateTime CreatedAt { get; set; }

        [DataMember]
        DateTime UpdatedAt { get; set; }
    }

    public class IdEqualityComparer<T> : IEqualityComparer<T> where T : IIdEntity {

        public bool Equals (T x, T y) => x.Id == y.Id;
        public int GetHashCode (T obj) => obj.Id.GetHashCode ();

    }

}
