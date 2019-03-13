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

namespace Limaki.UnitsOfWork.Model.Dto {

    [DataContract]
    public partial class StringTridle : IdEntity, IStringTridle {

        [DataMember]
        public virtual Guid Key { get; set; }

        [DataMember]
        public virtual Guid Member { get; set; }

        [DataMember]
        public virtual string Value { get; set; }

    }

    [DataContract]
    public partial class NumberTridle : IdEntity, INumberTridle {

        [DataMember]
        public virtual Guid Key { get; set; }

        [DataMember]
        public virtual Guid Member { get; set; }

        [DataMember]
        public virtual long Value { get; set; }

    }

    [DataContract]
    public partial class ByteArrayTridle : IdEntity, IByteArrayTridle {

        [DataMember]
        public virtual Guid Key { get; set; }

        [DataMember]
        public virtual Guid Member { get; set; }

        [DataMember]
        public virtual Byte[] Value { get; set; }

    }

    /// <summary>
    /// Byte array tridle without value
    /// </summary>
    [DataContract]
    public partial class ByteArrayTridleMemento : IdEntity, IByteArrayTridleMemento {

        [DataMember]
        public virtual Guid Key { get; set; }

        [DataMember]
        public virtual Guid Member { get; set; }

    }

    [DataContract]
    public partial class RelationTridle : IdEntity, IRelationTridle {

        [DataMember]
        public virtual Guid Key { get; set; }

        [DataMember]
        public virtual Guid Root { get; set; }

        [DataMember]
        public virtual Guid Leaf { get; set; }

    }
}
