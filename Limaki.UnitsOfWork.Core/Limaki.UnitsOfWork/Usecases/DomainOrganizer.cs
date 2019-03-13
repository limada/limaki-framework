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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Limaki.Common.Collections;
using Limaki.Common.Linqish;
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork {

    public class DomainOrganizer : IDomainOrganizer {

        protected EntityInteractor Interactor { get; set; } = new EntityInteractor ();

        public Guid RelationId<T> (Guid id, T value) where T : IIdEntity => Interactor.RelationId (id, value);
        public virtual void SetRelationId<E, M> (E entity, Expression<Func<E, M>> member) where M : IIdEntity => Interactor.SetRelationId (entity, member);
        public virtual void SetRelation<E, M> (E entity, Expression<Func<E, M>> member, M value) where M : IIdEntity => Interactor.SetRelation (entity, member, value);

        public IEnumerable<T> Relations<T> (IdentityMap map, Func<T, Guid> member, Guid id) => Interactor.Relations (map, member, id);
        public ICollection<T> CreateRelation<T> () => Interactor.CreateRelation<T> ();
        public bool CanChange<T> (IEnumerable<T> relations) => Interactor.CanChange (relations);
        public IEnumerable<T> AddToRelation<T> (IEnumerable<T> relations, T item) => Interactor.AddToRelation (relations, item);
        public IEnumerable<T> RemoveFromRelation<T> (IEnumerable<T> relations, T item) => Interactor.RemoveFromRelation (relations, item);

        IDictionary<Type, EntityInteractor> _interactors = null;
        IDictionary<Type, EntityInteractor> Interactors {
            get {
                if (_interactors == null) {

                    IEnumerable<(Type type, PropertyInfo prop)> InteractorProperties () {
                        var interactors = this.GetType ().GetProperties (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                              .Where (p => typeof (EntityInteractor).IsAssignableFrom (p.PropertyType));

                        foreach (var p in interactors) {
                            var u = p.PropertyType;
                            while (u != typeof (EntityInteractor) && !(u.IsGenericType && u.GetGenericTypeDefinition () == typeof (EntityInteractor<>))) {
                                u = u.BaseType;
                            }
                            if (u.IsGenericType)
                                yield return (u.GetGenericArguments ()[0], p);
                        }
                    }

                    _interactors = new Dictionary<Type, EntityInteractor> ();
                    foreach (var m in InteractorProperties ()) {
                        _interactors[m.type] = m.prop.GetValue (this) as EntityInteractor;
                    }
                }
                return _interactors;
            }
        }

        protected EntityInteractor<T> GetInteractor<T> () {
            if (Interactors.TryGetValue (typeof (T), out var interactor)) {
                return (EntityInteractor<T>)interactor;
            }
            return null;
        }

        public virtual T CreateEntity<T> (Store store) => (GetInteractor<T> () is EntityInteractor<T> interactor) ? interactor.Create (store) : store.AddCreated (store.Create<T> ());

        public virtual void Remove<T> (Store store, T entity) {
            if (GetInteractor<T> () is EntityInteractor<T> interactor)
                interactor.Remove (store, entity);
            else
                store.Remove (entity);
        }

        public virtual bool IsEmpty<T> (T entity) => (GetInteractor<T> () is EntityInteractor<T> interactor) ? interactor.IsEmpty (entity) : false;

        public virtual (bool can, FormattableString msg) CanRemove<T> (T entity) => (GetInteractor<T> () is EntityInteractor<T> interactor) ? interactor.CanRemove (entity) : (true, null);
        public virtual (bool can, FormattableString msg) CanRemove<T> (Store store, T entity) => (GetInteractor<T> () is EntityInteractor<T> interactor) ? interactor.CanRemove (store,entity) : (true, null);

        public virtual bool GotRelations (ListContainer container) {
            if (container == null)
                return false;
            if (Interactors.Count == 0)
                return false;
            var result = false;
            foreach (var interactor in Interactors.Values) {
                result = result || interactor.GotRelations (container);
            }
            return result;
        }

        public virtual void SetRelations (IdentityMap map) => Interactors.Values
                                                                         .ForEach (interactor => interactor.SetRelations (map));

        public virtual void SetRelationIds (IdentityMap map) => Interactors.Values.ForEach (interactor => interactor.SetRelationIds (map));

        public virtual void SaveChanges () { }

        public ILock CreateLock<E> (E entity) => (entity is IIdEntity idEntity) ? GetInteractor<E> ().GetLock (idEntity) : null;
    }

}
