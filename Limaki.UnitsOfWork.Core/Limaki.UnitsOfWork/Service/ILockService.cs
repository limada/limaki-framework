/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.Service {
    
    public interface ILockService {
        bool IsLocked (Guid key, Guid member);
        bool Lock (ILock @lock);
        IEnumerable<ILock> Locks (Guid key, Guid member);
        bool UnLock (ILock @lock);
        bool UnLock (Guid key, Guid member);
    }

}
