/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Limaki.Common;
using Limaki.UnitsOfWork.IdEntity.Data;

namespace Limaki.UnitsOfWork.Data {

    public class QuoreComposerBase<TQuore, TMapper> : EntityRepository
    where TQuore : IEntityQuore
    where TMapper : EntityQuoreMapper, new() {

        protected EntityQuoreMapper Mapper = new TMapper ();

        public override ILog Log { get => base.Log??(base.Log = Registry.Pool.TryGetCreate<Logger> ().Log (this.GetType ())); set => base.Log = value; }

        protected IEnumerable<(Type type, PropertyInfo ore)> QuoreProperties =>
            typeof (TQuore).GetProperties ().Where (p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition () == typeof (IQueryable<>))
                           .Select (p => ValueTuple.Create (p.PropertyType.GetGenericArguments ()[0], p));
    }
}
