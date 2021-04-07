/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.Common.Collections;
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork {

    public class UnitsOfWorkModelGuids : GuidFlags, ITypedGuidMapper {

        public UnitsOfWorkModelGuids () { }

        public UnitsOfWorkModelGuids (params Guid [] flags) { Add (flags); }

        public static UnitsOfWorkModelGuids operator | (UnitsOfWorkModelGuids c1, Guid g) => With (c1, g);
        public static implicit operator UnitsOfWorkModelGuids (Guid value) => new UnitsOfWorkModelGuids (value);
    }
}
