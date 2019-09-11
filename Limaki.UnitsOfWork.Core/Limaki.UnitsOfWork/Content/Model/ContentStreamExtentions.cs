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

using System.IO;
using Limaki.UnitsOfWork.Model;
using Limaki.UnitsOfWork.Tridles.Model;
using Limaki.UnitsOfWork.Tridles.Usecases;

namespace Limaki.UnitsOfWork.Content.Model {

    public static class ContentStreamExtentions {

        public static Content<Stream> ContentOf (this IContentStream contentStream, IByteArrayTridle tridle) {
            if (contentStream != null) {
                return new Content<Stream> {
                    Data = tridle.GetDeCompressed (contentStream.Compression),
                    Compression = contentStream.Compression,
                    ContentType = contentStream.ContentType
                };
            }
            return default;
        }
    }

}
