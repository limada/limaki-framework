/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2010 Lytico
 *
 * http://www.limada.org
 * 
 */

namespace Limaki.Common {
    public class Wrapper<T> {
        public Wrapper(T item) {
            this.Item = item;
        }

        public virtual T Item {
            get; 
            protected set;
        }
    }
}