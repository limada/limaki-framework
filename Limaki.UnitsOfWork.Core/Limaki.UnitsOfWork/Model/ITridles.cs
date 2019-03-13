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
using System.IO;
using System.Runtime.Serialization;

namespace Limaki.UnitsOfWork.Model {

    public interface IStringTridle : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Member { get; set; }

        [DataMember]
        string Value { get; set; }
    }

    public interface INumberTridle : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Member { get; set; }

        [DataMember]
        long Value { get; set; }
    }

    public interface IGuidTridle : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Member { get; set; }

        [DataMember]
        Guid Value { get; set; }
    }

    public interface IByteArrayTridle : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Member { get; set; }

        [DataMember]
        Byte[] Value { get; set; }
    }

    public interface IByteArrayTridleMemento : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Member { get; set; }

    }

    public interface IRelationTridle : IIdEntity {
        [DataMember]
        Guid Key { get; set; }

        [DataMember]
        Guid Root { get; set; }

        [DataMember]
        Guid Leaf { get; set; }
    }
}

