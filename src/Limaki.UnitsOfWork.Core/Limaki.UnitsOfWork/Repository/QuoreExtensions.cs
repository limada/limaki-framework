/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Linq;
using Limaki.Repository;

namespace Limaki.UnitsOfWork {

    public static class QuoreExtensions {

        public static IQueryable<E> QueryableOf<E> (this IDomainQuore quore) => quore.QueryableOf (typeof (E)) as IQueryable<E>;

        /// <summary>
        /// matching IQueryable the of <see cref="IDomainQuore"/>
        /// </summary>
        /// <returns>matching IQueryable, matching Type</returns>
        public static IQueryable QueryableOf (this IDomainQuore quore, Type matchType) {

            var queryableProperty = quore.GetType ().GetProperties ().FirstOrDefault (
                         p => p.PropertyType.IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>)
                         && p.PropertyType.GenericTypeArguments[0] == matchType);

            return queryableProperty?.GetValue (quore) as IQueryable;
        }
    }

}
