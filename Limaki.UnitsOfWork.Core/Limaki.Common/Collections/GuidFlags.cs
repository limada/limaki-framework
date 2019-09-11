/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;

namespace Limaki.Common.Collections {

    public class GuidFlags : Flags<Guid> {

        public GuidFlags () { }
        public GuidFlags (params Guid [] flags) { Add (flags); }
        public static GuidFlags __ (params Guid [] flags) => new GuidFlags (flags);
        public static implicit operator GuidFlags (Guid value) => new GuidFlags (value);
        public static implicit operator GuidFlags (Guid [] value) => new GuidFlags (value);

        public static GuidFlags operator | (GuidFlags c1, Guid g) => With (c1, g);
        public static GuidFlags operator | (GuidFlags c1, GuidFlags g) => With (c1, g);

    }
}
