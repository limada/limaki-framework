/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Collections.Generic;

namespace Limaki.UnitsOfWork.SanityCheck
{
    public interface ISanityCheckService
    {
        IEnumerable<SanityCheckResult> SanityChecks(IEnumerable<ICriterias> criterias);
    }
}