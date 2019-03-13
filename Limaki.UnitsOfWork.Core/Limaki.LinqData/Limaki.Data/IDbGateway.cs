/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Data;
using Limaki.Common;

namespace Limaki.Data {

    public interface IDbGateway : IGateway { 
        IDbProvider Provider { get; }
    }
    
}