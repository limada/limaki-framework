/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.Content.Model {

    public interface IContentStream : IIdEntity {

        [DataMember]
        Guid Compression { get; set; }

        [DataMember]
        Guid ContentType { get; set; }

        /// <summary>
        /// id of content-stream
        /// eg. IByteArrayTridle.Id
        /// </summary>
        /// <value>The data identifier.</value>
        [DataMember]
        Guid StreamId { get; set; }

        [DataMember, Display]
        string Description { get; set; }

        /// <summary>
        /// filename, url etc.
        /// </summary>
        /// <value>The source.</value>
        [DataMember, Display]
        string Source { get; set; }
    }

}
