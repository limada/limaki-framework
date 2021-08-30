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

using System;
namespace Limaki.Common {
    /// <summary>
    /// 
    /// </summary>
    public interface ICheckable {
        bool Check();
    }

    public class CheckFailedException : ArgumentException {
        public CheckFailedException(string message):base(message){}

        public CheckFailedException(Type source, Type needed) :
            base(source.Name + " needs a " + needed.Name) { }
    }
}