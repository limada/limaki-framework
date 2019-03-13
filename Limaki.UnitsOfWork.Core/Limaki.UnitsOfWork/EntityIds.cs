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

using System.Runtime.Serialization;
using System.Collections.Generic;
using Limaki.Common.Reflections;
using System;
using System.Linq;

namespace Limaki.UnitsOfWork {

    public abstract class EntityIds {

        public abstract Guid GetIndex (string name);

        /// <summary>
        /// index = Relations
        /// Count = count of items
        /// </summary>
        [DataMember]
        public IDictionary<Guid, IEnumerable<Guid>> Ids { get; set; } = new Dictionary<Guid, IEnumerable<Guid>> ();

        public IEnumerable<Guid> IdsOf<T> () {

            var name = new TypeInfo { Type = typeof (T) }.ImplName;

            if (Ids.TryGetValue (GetIndex (name), out var ids)) {
                return ids;
            }
            return new Guid[0];
        }

        public virtual int Count () => Ids.Values.Sum (i => i.Count ());
    }
}
