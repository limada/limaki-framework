/*
 * Limada 
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2010-2017 Lytico
 *
 * http://www.limada.org
 * 
 */


namespace Limaki.Data {

    public interface IDbModelBuilder {
        bool CheckTable<T> (IGateway gateway);
        bool CheckIndices<T> (IGateway gateway);
        void CheckModel<T> (IGateway gateway);
    }
}
