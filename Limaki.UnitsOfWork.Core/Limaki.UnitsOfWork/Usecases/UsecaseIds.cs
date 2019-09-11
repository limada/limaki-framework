/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Limaki.UnitsOfWork.Usecases {
    
    public class UsecaseIds {
        
        public IEnumerable<Guid> Ids {
            get {
                return GetType ().GetProperties ()
                          .Where (p => p.PropertyType == typeof (Guid))
                          .Select (p => (Guid)p.GetValue (this))
                                 .Where (g => g != Guid.Empty);
            }
        }

        public string Name (Guid id) {
            return (GetType ()
                .GetProperties ()
                .FirstOrDefault (p => p.PropertyType == typeof (Guid) && ((Guid)p.GetValue (this)) == id)
                ?.GetCustomAttributes (typeof (DisplayAttribute), false)
                .FirstOrDefault () as DisplayAttribute)
                ?.Name;


        }
    }
}
