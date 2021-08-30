/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */


using Limaki.Common;
using Limaki.Data;

namespace Limaki.Repository {

    public interface IDomainQuoreFactory<T> where T : IDomainQuore {
        bool Supports (IDbProvider provider);
        IDbGateway CreateGateway (IDbProvider provider);
        IQuore CreateQuore (IDbGateway gateway);
        T CreateDomainQuore (Iori iori);
    }

}
