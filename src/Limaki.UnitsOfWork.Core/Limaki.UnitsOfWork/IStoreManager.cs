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

using System.Collections.Generic;
using Limaki.Common;

namespace Limaki.UnitsOfWork {
    
    public interface IStoreManager {
        
        Store Store { get; }
        Store CreateStore();

        ILog Log { get; set; }

        void Clear ();
        void CollectEntities (Store store, object item);
        bool CollectEntity<T> (Store store, T item);
        void ExpandItems (IListContainer container, IdentityMap map);
        void InstrumentFactory (IFactory afactory);
        void SaveTo (Store store, ChangeSetContainer container);
        void SaveTo (ChangeSetContainer container);

    }
}