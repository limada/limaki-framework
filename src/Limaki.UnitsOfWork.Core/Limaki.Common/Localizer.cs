/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2016 - 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Limaki.Common {

    public class Localizer {

        public string this[string item] {
            get { return item; }
        }
        public static string Get(string item) {
            return item;
        }
    }

}
