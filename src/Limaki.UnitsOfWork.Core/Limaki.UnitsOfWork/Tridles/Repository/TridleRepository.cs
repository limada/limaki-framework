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
using Limaki.Common.Linqish;
using Limaki.UnitsOfWork.Repository;
using Limaki.UnitsOfWork.IdEntity.Repository;
using Limaki.UnitsOfWork.Tridles.Model;
using Limaki.UnitsOfWork.Tridles.Model.Dto;
using Limaki.UnitsOfWork.Tridles.Usecases;

namespace Limaki.UnitsOfWork.Tridles.Repository {

    public class TridleRepository : EntityRepository {

        public class Querables : QuerablesBase {

            public IQueryable<StringTridle> StringTridles { get; set; }

            public IQueryable<NumberTridle> NumberTridles { get; set; }

            public IQueryable<ByteArrayTridle> ByteArrayTridles { get; set; }
            public IQueryable<ByteArrayTridleMemento> ByteArrayTridleMementos { get; set; }

            public IQueryable<RelationTridle> RelationTridles { get; set; }

        }

        /// <summary>
        /// ensure that ByteArrayTridle is called before ByteArrayTridleMemento
        /// </summary>
        public static IEnumerable<Guid> Ensure (ITridleQuore quore) => quore.ByteArrayTridles.Select (b => b.Id).Take (1).ToArray ();

        public virtual void AddToMap (IdentityMap map, Querables sources) {

            var factory = map.ItemFactory;
            AddToMap<IStringTridle> (map, sources.StringTridles?.Distinct ());
            AddToMap<INumberTridle> (map, sources.NumberTridles?.Distinct ());
            AddToMap<IRelationTridle> (map, sources.RelationTridles?.Distinct ());
            AddToMap<IByteArrayTridle> (map, sources.ByteArrayTridles?.Distinct ());
            AddToMap<IByteArrayTridleMemento> (map, sources.ByteArrayTridleMementos?.Distinct ());


        }

        public virtual void FillContainer (ITridleContainer result, IdentityMap map) {
            result.Set (map.Stored<IStringTridle> ()?.ToArray ());
            result.Set (map.Stored<INumberTridle> ()?.ToArray ());
            result.Set (map.Stored<IRelationTridle> ()?.ToArray ());
            result.Set (map.Stored<IByteArrayTridle> ()?.ToArray ());
            result.Set (map.Stored<IByteArrayTridleMemento> ()?.ToArray ());

        }

        public Querables ComposeQuerables (ITridleQuore quore, ITridleCriterias preds) {

            var query = new Querables { };

            query.StringTridles = quore.StringTridles.WhereIf (ConvertPredicate<IStringTridle, StringTridle> (preds.StringTridles));
            query.NumberTridles = quore.NumberTridles.WhereIf (ConvertPredicate<INumberTridle, NumberTridle> (preds.NumberTridles));
            query.RelationTridles = quore.RelationTridles.WhereIf (ConvertPredicate<IRelationTridle, RelationTridle> (preds.RelationTridles));
            query.ByteArrayTridles = quore.ByteArrayTridles.WhereIf (ConvertPredicate<IByteArrayTridle, ByteArrayTridle> (preds.ByteArrayTridles));
            query.ByteArrayTridleMementos = quore.ByteArrayTridleMementos.WhereIf (ConvertPredicate<IByteArrayTridleMemento, ByteArrayTridleMemento> (preds.ByteArrayTridleMementos));

            return query;
        }

        public C LoadTridles<C> (ITridleQuore quore, ITridleCriterias preds) where C:ITridleContainer, new() => LoadTridles (new C (), quore, preds);

        public C LoadTridles<C> (C container, ITridleQuore quore, ITridleCriterias preds) where C : ITridleContainer {

            var query = ComposeQuerables (quore, preds);

            Log.Debug ($"{nameof (LoadTridles)} {query.Info ()}");
            AddToMap (container.Map, query);
            FillContainer (container, container.Map);

            return container;
        }
    }


}
