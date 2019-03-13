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
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {

    public abstract class StoreManager : IStoreManager {

        protected abstract Store CreateStore ();

        protected abstract IFactory CreateFactory ();

        public abstract void CollectEntities (Store store, object item);

        public abstract void ExpandItems (ListContainer container, IdentityMap map);

        protected Store _store = null;
        public virtual Store Store {
            get {
                if (_store == null) {
                    _store = CreateStore ();
                    var factory = CreateFactory ();
                    InstrumentFactory (factory);
                    _store.ItemFactory = factory;
                }
                return _store;
            }
        }

        public virtual void InstrumentFactory (IFactory afactory) { }

        public virtual void Clear () {
            if (_store != null) {
                Store.Dispose ();
                _store = null;
            }
        }

        public ILog Log { get; set; }

        public virtual bool CollectEntity<T> (Store store, T item) {
            if (item != null) {
                store.Refresh (item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// saves Store.StateMap{T} to Container
        /// used in SaveTo (Container)
        /// </summary>
        /// <param name="store">Store.</param>
        /// <param name="container">Container.</param>
        protected virtual void SaveTo<T> (Store store, ChangeSetContainer container) {

            var changeset = container.ChangeSet<T> ();

            var copier = new Copier<T> (CopierOptions.DataMember);
            var factory = store.ItemFactory;

            Func<T, T> MakeDTO = e => {
                var dto = copier.Copy (e, factory.Create<T> ());
                //var flatten = dto as IFlattable;
                //if (flatten != null)
                //  flatten.Flatten ();
                return dto;
            };

            var createList = store.Created<T> ();

            foreach (var child in createList) {
                changeset.Updated.Add (MakeDTO (child));
            }
            foreach (var child in store.Updated<T> ()) {
                if (!createList.Contains (child))
                    changeset.Updated.Add (MakeDTO (child));
            }
            foreach (var child in store.Removed<T> ()) {
                changeset.Removed.Add (MakeDTO (child));
            }

            if (changeset.HasData)
                Log.Debug ($"{nameof (SaveTo)}<{typeof (T).Name}>");
        }

        protected static MethodInfo GenericSaveTo { get; set; }

        /// <summary>
        /// Saves Store.StateMap to container
        /// </summary>
        /// <param name="container">Container.</param>
        public virtual void SaveTo (Store store, ChangeSetContainer container) {

            Log.Debug ($"{nameof (SaveTo)} {container.Info ()}");

            if (GenericSaveTo == null) {
                Expression<Action<ChangeSetContainer>> method = c => this.SaveTo<object> (store, c);
                GenericSaveTo = ((MethodCallExpression)((LambdaExpression)method).Body).Method.GetGenericMethodDefinition ();
            }

            foreach (var prop in container.ChangeSetProperties ()) {
                var meth = GenericSaveTo.MakeGenericMethod (prop.PropertyType.GetGenericArguments ()[0]);
                meth.Invoke (this, new object[] { store, container });
            }
        }

        public virtual void SaveTo (ChangeSetContainer container) => SaveTo (Store, container);

    }

    public abstract class StoreManager<S, F> : StoreManager where S : Store, new() where F : IFactory, new() {

        public new S Store => (S)base.Store;

        protected override IFactory CreateFactory () => new F ();

        protected override Store CreateStore () => new S { ItemFactory = CreateFactory () };

        public override void SaveTo (Store store, ChangeSetContainer container) {
            base.SaveTo (store, container);
        }

    }
}
