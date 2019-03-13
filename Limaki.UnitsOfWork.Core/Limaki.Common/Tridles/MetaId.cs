/*
 * Tridles 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2015 Lytico
 *
 * http://www.limada.org
 * 
 */

namespace Limaki.Common.Tridles {
   
    /// <summary>
    /// defines the metadata constants
    /// </summary>
    /// <typeparam name="K"></typeparam>
    public class MetaId<K> {
        /// <summary>
        /// Id of a type
        /// stored as key of a type tridle
        /// </summary>
        public K Type { get; set; }

        /// <summary>
        /// name of a type
        /// stored as member of a type tridle
        /// the value is typeof(clazz).FullName
        /// </summary>
        public K TypeName { get; set; }

        /// <summary>
        /// member of a type
        /// stored as member of a tridle
        /// the value is nameof(type.Member)
        /// the key is the Id of the type-tridle
        /// </summary>
        public K TypeMember { get; set; }

        /// <summary>
        /// dynamic property of a class
        /// stored as member of a tridle
        /// the value is name of the dynamic property
        /// the key is the Id of the type-tridle
        /// </summary>
        public K Member { get; set; }

        /// <summary>
        /// type of dynamic property of a class
        /// stored as member of a tridle
        /// the value is the Type.FullName of the property type
        /// the key is the id of the DynId-tridle
        /// </summary>
        public K MemberType { get; set; }

        public override string ToString () {
            // TODO: make formatstring static to avoid typecheck
            if (typeof (K) == typeof (long))
                return string.Format ($"{{{nameof (Type)} = {Type:X16} {nameof (TypeName)} = {TypeName:X16} {nameof (TypeMember)} = {TypeMember:X16} {nameof (Member)} = {Member:X16} {nameof (MemberType)} = {MemberType:X16}}}");
            return string.Format ($"{{{nameof(Type)} = {Type} {nameof (TypeName)} = {TypeName} {nameof (TypeMember)} = {TypeMember} {nameof (Member)} = {Member} {nameof (MemberType)} = {MemberType}}}");
        }
    }
}