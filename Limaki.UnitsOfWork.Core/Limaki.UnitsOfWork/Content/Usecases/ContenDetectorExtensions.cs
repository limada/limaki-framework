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
using System.Collections.Generic;
using System.Linq;

namespace Limaki.UnitsOfWork.Content.Usecases {

    public static class ContenDetectorExtensions {

        public static ContentInfo Find (this IEnumerable<ContentInfo> it, string extension) {
            extension = extension.ToLower ().TrimStart ('.');
            return it.FirstOrDefault (type => type.Extension == extension);
        }

        public static ContentInfo FindMime (this IEnumerable<ContentInfo> it, string mime) {
            mime = mime.ToLower ();
            return it.FirstOrDefault (type => type.MimeType == mime);
        }

        public static ContentInfo Find (this IEnumerable<ContentInfo> it, Guid contentType) => it.FirstOrDefault (type => type.ContentType == contentType);

        public static bool Supports (this IEnumerable<ContentInfo> it, string extension) => Find (it, extension) != null;

        public static bool Supports (this IEnumerable<ContentInfo> it, Guid contentType) => Find (it, contentType) != null;
    }
}
