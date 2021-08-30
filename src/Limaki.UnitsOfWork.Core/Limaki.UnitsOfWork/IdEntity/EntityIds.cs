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
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork.IdEntity {

    public abstract class EntityIds {

        public abstract Guid GetIndex (Type type);

        /// <summary>
        /// key = Guid of Type
        /// Value = <see cref="IIdEntity.Id"/>s
        /// </summary>
        [DataMember]
        public IDictionary<Guid, IEnumerable<Guid>> Ids { get; set; } = new Dictionary<Guid, IEnumerable<Guid>> ();

        public IEnumerable<Guid> IdsOf<T> () {
            if (typeof (T).IsInterface) {
                // TODO: get dto-type
            }
            if (Ids.TryGetValue (GetIndex (typeof (T)), out var ids)) {
                return ids;
            }
            return new Guid[0];
        }

        public virtual int Count () => Ids.Values.Sum (i => i.Count ());
    }
}
