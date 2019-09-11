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

namespace Limaki.UnitsOfWork {

    /// <summary>
    /// used to specify a Guid for class
    /// to use in <see cref="Model.SchemaGuids.IModelGuids"/>
    /// </summary>
    // TODO: make use of TypeGuidAttribute
    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class TypeGuidAttribute : Attribute {
        public TypeGuidAttribute (Type type) {
            Type = type;
        }
        public Type Type { get; }
    }

}
