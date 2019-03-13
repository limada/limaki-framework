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

namespace Limaki.UnitsOfWork.Cms {

    public class ContentInfo {

        public ContentInfo (string description, Guid contentType, string extension, string mimeType, Guid compression) {
            ContentType = contentType;
            Extension = extension;
            MimeType = mimeType;
            Description = description;
            Compression = compression;
        }

        public ContentInfo (string description, Guid contentType, string extension, string mimeType, Guid compression, Magic[] magics)
            : this (description, contentType, extension, mimeType, compression) {
            Magics = magics;
        }

        public virtual Guid Compression { get; protected set; }
        public virtual Guid ContentType { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual string Extension { get; protected set; }
        public virtual string MimeType { get; protected set; }
        public virtual Magic[] Magics { get; protected set; }

        public bool HasMagics => Magics != null && Magics.Length > 0;
    }
}
