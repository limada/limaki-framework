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
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork.Content.Model.Dto {

    [DataContract]
    public partial class ContentStream : IdEntity.Model.Dto.IdEntity, IContentStream {

        [DataMember]
        public virtual Guid Compression { get; set; }

        [DataMember]
        public virtual Guid ContentType { get; set; }

        [DataMember]
        public virtual Guid StreamId { get; set; }

        [DataMember]
        public virtual string Description { get; set; }

        [DataMember]
        public virtual string Source { get; set; }

    }
}
