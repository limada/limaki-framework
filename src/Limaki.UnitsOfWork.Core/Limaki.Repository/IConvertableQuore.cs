/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Linq.Expressions;

namespace Limaki.Repository {

    public interface IConvertableQuore : IQuore {
        Func<Expression, Type, Expression> Convert { get; set; }
    }

}