/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;

namespace Limaki.UnitsOfWork {
    public interface IListContainer : IDisposable {
        void Set<T> (IEnumerable<T> value);
        IEnumerable<Type> KnownTypes ();
        bool HasList<T> ();
        IEnumerable<T> List<T> ();
        void Clear ();
    }
}