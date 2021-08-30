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
using System.Linq;

namespace Limaki.UnitsOfWork.Data {

    public class CachedQuery<T> {

        public CachedQuery (IQueryable<T> queryable) {
            _queryable = queryable;
        }

        IEnumerable<T> _cache = null;
        IQueryable<T> _queryable = null;

        public IQueryable<T> Query => _queryable;
        public bool IsNull => _queryable == null;

        public IEnumerable<T> Cached () => _cache ??
        (
            _cache = _queryable?.ToArray ()
        );

    }

}
