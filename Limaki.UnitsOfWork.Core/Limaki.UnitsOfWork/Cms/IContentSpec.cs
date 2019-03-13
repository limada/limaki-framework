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

namespace Limaki.UnitsOfWork.Cms {

    public interface IContentSpec {
        IEnumerable<ContentInfo> ContentSpecs { get; }
        ContentInfo Find (string extension);
        ContentInfo FindMime (string mime);
        ContentInfo Find (Guid contentType);
        bool Supports (string extension);
        bool Supports (Guid contentType);
    }
}
