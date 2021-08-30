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

    /// <summary>
    /// Service facade.
    /// used to consume a servcice on client side
    /// </summary>
    public interface IClientUsecase {
        Store Store { get; }
        void SaveChanges ();

        string ServiceInfo ();
        string Connect();

        void SaveChanges (Store store);
        void LoadOnStart ();
        void ClearOnEnd ();

        Type ServiceType { get; }
    }

    public interface IServiceClient {
        IClientUsecase ServiceClient { get; }
    }
}
