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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Limaki.Common.Reflections;

namespace Limaki.UnitsOfWork.IdEntity {

    public abstract class EntityCounts {

        public abstract Guid GetIndex (Type type);

        /// <summary>
        /// index = Relations
        /// Count = count of items
        /// </summary>
        [DataMember]
        public IDictionary<Guid, long> Counts { get; set; } = new Dictionary<Guid, long> ();

        public virtual long Count<T> () {
            if (Counts.TryGetValue (GetIndex (typeof(T)), out var count)) {
                return count;
            }

            return -1;
        }

        public long Count () => Counts.Count == 0 ? -1 : Counts.Values.Sum ();

        public bool Any<T> () => Count<T> () > 0;

        public bool Any () => Count () > 0;
    }

}