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
using System.IO;

namespace Limaki.UnitsOfWork {

    public interface ICompressionWorker {
        bool Compressable (Guid compression);
        Stream Compress (Stream stream, Guid compression);
        Stream DeCompress (Stream stream, Guid compression);
    }
}
