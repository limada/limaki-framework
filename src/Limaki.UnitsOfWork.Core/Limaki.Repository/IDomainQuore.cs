/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;

namespace Limaki.Repository {

    /// <summary>
    /// base interface for domain specific quore
    /// </summary>
    public interface IDomainQuore : IDisposable {
        
        IQuore Quore { get; }

    }
    
}
