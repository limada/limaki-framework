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
using Limaki.Common;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork.IdEntity.Model {
    
    public abstract class EntityFactory : Factory {
        
        public virtual void AddIdEntityStuff<T> (IFactory factory)
           where T : IIdEntity {

            factory.Add<IEqualityComparer<T>> (() => new IdEqualityComparer<T> ());

            factory.Add<IIdentityList<T>> (
                () => new IdentityList<Guid, T> (e => e.Id)
                );
        }

        IList<Tuple<Type, Type>> _entityMapping = new List<Tuple<Type, Type>> ();
        public IList<Tuple<Type, Type>> EntityMapping {
            get {
                if (_clazzes == null)
                    InstrumentClazzes ();
                return _entityMapping;
            }
        }

        public virtual void InstrumentEntity<I, T> (IFactory factory, bool mapping = true) where I : IIdEntity where T : Dto.IdEntity, I, new() {

            Instrument<I, T> (factory, mapping);
            AddIdEntityStuff<I> (factory);
        }

        public virtual void Instrument<I, T> (IFactory factory, bool mapping = true) where T : I, new() {

            factory.Add<I> (() => new T ());
            knownClazzes [typeof (I)] = typeof (T);

            if (mapping) {
                if (!EntityMapping.Any (e => e.Item1 == typeof (I)))
                    EntityMapping.Add (Tuple.Create (typeof (I), typeof (T)));
            }
        }

        protected override void InstrumentClazzes () {

            base.InstrumentClazzes ();

            InstrumentDTO (this);

        }

        public Store Store { get; set; }

        public Func<object[], T> ViewModelFunc<T, I> (IFactory factory) where T : ViewModel<I> {
            return e => {
                if (e == null)
                    e = new object[] { factory.Create<I> () };
                var vm = Activator.CreateInstance (typeof (T), e) as T;
                vm.Store = this.Store;
                return vm;
            };
        }

        public void AddViewModel<M, E> (IFactory factory) where M : ViewModel<E> {
            factory.Add (ViewModelFunc<M, E> (factory));
            factory.Add<IViewModel<E>> (ViewModelFunc<M, E> (factory));
        }

        public override void Clear () {
            base.Clear ();
            _entityMapping?.Clear ();
        }

        public abstract void InstrumentDTO (IFactory factory);
    }
}
