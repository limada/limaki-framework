/*
 * Tridles 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2015 Lytico
 *
 * http://www.limada.org
 * 
 */

namespace Limaki.Common.Tridles {

    public interface ITridle<K> {
        K Id { get; set; }
        K Key { get; set; }
        K Member { get; set; }
    }

    public interface ITridle<K, V>:ITridle<K> {
        V Value { get; set; }
    }
}