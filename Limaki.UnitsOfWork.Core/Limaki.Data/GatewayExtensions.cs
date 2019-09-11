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

using System.Collections.Generic;

namespace Limaki.Data {
    public static class GatewayExtensions {

        public static long BulkCopy<T> (this IGatewayExtended it, IEnumerable<T> source) where T : class => it?.BulkCopy<T> (source, null) ?? -1;
    }
}