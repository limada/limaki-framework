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
using System.Collections.Generic;
using System.Linq;
using Limaki.Common;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork.Hash.Model {

    public abstract class HashFactory : Factory {

        public virtual void AddHashEntityStuff<T> (IFactory factory)
           where T : IHashEntity {

            factory.Add<IEqualityComparer<T>> (() => new HashEqualityComparer<T> ());

            factory.Add<IIdentityList<T>> (
                () => new IdentityList<string, T> (e => e.Hash)
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

        public virtual void InstrumentEntity<I, T> (IFactory factory, bool mapping = true) where I : IHashEntity where T : I, new() {

            factory.Add<I> (() => new T { });
            knownClazzes [typeof (I)] = typeof (T);

            AddHashEntityStuff<I> (factory);
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

        public Func<object [], T> ViewModelFunc<T, I> (IFactory factory) where T : ViewModel<I> {
            return e => {
                if (e == null)
                    e = new object [] { factory.Create<I> () };
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
