/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2010-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;

namespace Limaki.Common {

    public class EventArgs<T>:EventArgs {

        public EventArgs(T arg) {
            this.Arg = arg;
        }

        private T _arg;
        public T Arg {
            get { return _arg; }
            set { _arg = value; }
        }
    }
}
