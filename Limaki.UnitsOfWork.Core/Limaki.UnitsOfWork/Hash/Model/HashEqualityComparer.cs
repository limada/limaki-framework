/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2019 Lytico
 *
 * http://www.limada.org
 * 
 */


using System.Collections.Generic;

namespace Limaki.UnitsOfWork.Hash.Model {

    public class HashEqualityComparer<T> : IEqualityComparer<T> where T : IHashEntity {

        public bool Equals (T x, T y) => x.Hash == y.Hash;
        public int GetHashCode (T obj) => obj.Hash.GetHashCode ();

    }


}
