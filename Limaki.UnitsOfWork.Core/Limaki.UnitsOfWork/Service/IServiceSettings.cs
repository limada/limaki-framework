/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2008-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
namespace Limaki.Common.Services {
    public interface IServiceSettings {
        int Port { get; set; }
        string Prefix { get; set; }
        string IP { get; set; }
        BindingFlag Binding { get; set; }
        string Info();
        TimeSpan Timeout { get; set; }
    }
}