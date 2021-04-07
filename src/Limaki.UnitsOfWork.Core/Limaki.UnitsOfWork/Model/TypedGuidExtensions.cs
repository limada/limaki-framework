/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Limaki.UnitsOfWork.Model {

    public static class TypedGuidExtensions {

        static readonly BindingFlags BindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        /// <summary>
        /// type = <see cref="TypeGuidAttribute.Type"/>
        /// id = Guid of the Type
        /// </summary>
        public static IEnumerable<(Type type, Guid id)> TypeGuidMapping (this ITypedGuidMapper typedGuidMapper) =>
            typedGuidMapper.GetType ().GetProperties (BindingFlags)
               .Where (p => p.PropertyType == typeof(Guid) && p.IsDefined (typeof(TypeGuidAttribute)))
               .SelectMany (p => {
                    var guid = (Guid)p.GetValue (null);

                    return p.GetCustomAttributes<TypeGuidAttribute> ().Select (tg => (tg.Type, guid));
                });

        static Type InterfaceOf (Type clazz) => clazz.IsInterface ? clazz : clazz.GetInterfaces ().FirstOrDefault (i => i.Name == $"I{clazz.Name}");

        static IEnumerable<Type> InterfacesOf (Type clazz) => clazz.IsInterface ? new[] {clazz} : clazz.GetInterfaces ().Where (i => i.Name == $"I{clazz.Name}");

        /// <summary>
        /// type = interface of <see cref="TypeGuidAttribute.Type"/> where interface-name == I + Type.Name
        /// id = Guid of the Type
        /// </summary>
        public static IEnumerable<(Type type, Guid id)> TypeGuidInterfaceMapping (this ITypedGuidMapper modelGuids) =>
            modelGuids.TypeGuidMapping ().SelectMany (tg => InterfacesOf (tg.type).Select (i => (i, tg.id)));

        public static IEnumerable<(Type interfaze, Type clazz)> TypeMapping (this ITypedGuidMapper modelGuids) =>
            modelGuids.TypeGuidMapping ().SelectMany (tg => InterfacesOf (tg.type).Select (i => (i, tg.type)));

    }

}