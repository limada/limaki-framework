/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

namespace Limaki.Data {

    public interface ILinqToDBModelBuilder : IDbModelBuilder {

        // void BuildModel (DataConnection context);

        bool CheckTable<T> (LinqToDBGateway gateway);
        bool CheckIndices<T> (LinqToDBGateway gateway);
        void CheckModel<T> (LinqToDBGateway gateway);

   }

    
}
