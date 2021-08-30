/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace System.Xml.Linq {

    public static class XElementExtensions {

        public static XElement ElementByName (this XElement it, string name) => it?.Elements ()?.FirstOrDefault (l => l.Name.LocalName == name);
        public static XElement ElementByName (this IEnumerable<XElement> it, string name) => it?.FirstOrDefault (l => l.Name.LocalName == name);

        public static decimal AsDecimal (this XElement e) {
            if (e != null && decimal.TryParse (e.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
                return r;
            else return default (decimal);

        }
    }
}
