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

namespace Limaki.UnitsOfWork.Model.Dto {
    public class Lock : IdEntity, ILock {
        [DataMember]
        public Guid Key { get; set; }

        [DataMember]
        public Guid Member { get; set; }

        [DataMember]
        public string MachineName { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public DateTime ReleasedAt { get; set; }
    }

}
