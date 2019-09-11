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

namespace Limaki.UnitsOfWork.Service.Client {
    
    public interface IClientUsecase {
        Store Store { get; }
        void SaveChanges ();
        string ServiceInfo ();
        void SaveChanges (Store store);
        void Connect ();
        void LoadOnStart ();
        void ClearOnEnd ();
        Type ServiceType { get; }
    }

    public interface IServiceClient {
        IClientUsecase ServiceClient { get; }
    }
}
