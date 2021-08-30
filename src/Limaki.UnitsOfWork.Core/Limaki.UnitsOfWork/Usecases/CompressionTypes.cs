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
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork.Usecases {

    public class CompressionTypes : GuidFlags {

        public static Guid None { get; private set; } = new Guid ("3efec4cd-82fe-49a2-a7e9-9cab59e96198");
        public static Guid Zip { get; private set; } = new Guid ("c0bc6eb5-722a-4ad8-a5ea-90888c0877e7");
        public static Guid BZip2 { get; private set; } = new Guid ("209f1517-799f-4e5a-9043-60b69f809c4c");
        public static Guid NeverCompress { get; private set; } = new Guid ("c605b520-89ce-4840-ada8-a3cdb79b202e");

    }
}
