/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2014 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Limaki.Data {

    /// <summary>
    /// mapping methods to translate the calls of an <see cref="IQuore"/>
    /// used to map entity-interfaces to entity-classes or entity-classes to data-object classes
    /// </summary>
    public interface IQuoreMapper {

        /// <summary>
        /// maps expressions to dataobject type expressions
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        Expression Map (Expression arg, Type queryType);

        /// <summary>
        /// maps base type to dataobject type
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        Type MapIn (Type baseType);

        /// <summary>
        /// maps <see cref="IQuore"/> parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        IEnumerable<T> MapIn<T> (IEnumerable<T> entities);

        /// <summary>
        /// maps <see cref="IQuore"/>.Query{T} calls 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <returns></returns>
        IQueryable<T> MapQuery<T> (IQuore store);
    }
}