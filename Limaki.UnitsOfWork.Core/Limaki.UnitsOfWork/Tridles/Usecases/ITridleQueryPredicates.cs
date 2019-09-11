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
using System.Linq.Expressions;
using Limaki.UnitsOfWork.Tridles.Model;

namespace Limaki.UnitsOfWork.Tridles.Usecases {
    
    public interface ITridleQueryPredicates : IQueryPredicates {

        Expression<Func<IStringTridle, bool>> StringTridles { get; set; }
        Expression<Func<INumberTridle, bool>> NumberTridles { get; set; }
        Expression<Func<IRelationTridle, bool>> RelationTridles { get; set; }
        Expression<Func<IByteArrayTridle, bool>> ByteArrayTridles { get; set; }
        Expression<Func<IByteArrayTridleMemento, bool>> ByteArrayTridleMementos { get; set; }


    }
}
