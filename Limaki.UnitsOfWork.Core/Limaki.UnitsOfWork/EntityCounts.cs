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
using System.Linq;
using Limaki.Common.Reflections;
using System;

namespace Limaki.UnitsOfWork {

    public abstract class EntityCounts {

		public abstract Guid GetIndex (string name);

        /// <summary>
        /// index = Relations
        /// Count = count of items
        /// </summary>
        [DataMember]
		public IDictionary<Guid, long> Counts { get; set; } = new Dictionary<Guid, long> ();

        public long Count<T> () {

            var name = new TypeInfo { Type = typeof (T) }.ImplName;

            if (Counts.TryGetValue (GetIndex (name), out var count)) {
                return count;
            }
            return -1;
        }

        public long Count () => Counts.Count == 0 ? -1 : Counts.Values.Sum ();

        public bool Any<T> () => Count<T> () > 0;

        public bool Any () => Count () > 0;
    }
}
