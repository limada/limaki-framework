/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 - 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Limaki.Common;
using Limaki.Common.Collections;
using Limaki.Common.Linqish;
using Limaki.Common.Reflections;
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork.Data {

    public class EntityRepository {

        public EntityRepository () => Init ();

        public virtual void Init () { }

        public virtual ILog Log { get; set; }

        public IFactory Factory { get; set; }

        protected virtual int EntityCount { get; set; }
        protected bool FillObjectTree { get; set; }

        public Action<string> ProveError { get; set; }

        public virtual string Prove<T> (T item, bool canBeNull = false) {

            if (canBeNull || !object.Equals (item, default (T))) return null;

            var message = typeof (T).Name + " is null";
            ProveError?.Invoke (message);
            return message;

        }

        public virtual string Prove<S, M> (S source, M member, bool canBeNull) {

            if (canBeNull || !object.Equals (member, default (M))) return null;

            var message = typeof (M).Name + " is null";
            if (source is IIdEntity entity)
                message = string.Format ("{0}:{1}.{2}", typeof (S), entity.Id, message);

            ProveError?.Invoke (message);

            return message;
        }

        public virtual void AddToMapNoChecks<I> (IdentityMap map, IQueryable<I> source) where I : IIdEntity => AddToMap (map, source, null);

        public virtual void AddToMap<I> (IdentityMap map, IQueryable<I> source, Action<IdentityMap, I> afterAdd = null) where I : IIdEntity {

            if (source == null)
                return;
            if (!typeof (I).IsInterface) {
                Log.Debug ($"WARNING! {typeof (I).Name} is not an interface");
            }
            var copier = new Copier<I> ();
            var factory = map.ItemFactory;
            foreach (var item in source) {
                if (map.Contains<I, Guid> (item.Id)) continue;
                var add = copier.Copy (item, factory.Create<I> ());
                map.Add (add);
                afterAdd?.Invoke (map, add);
                EntityCount++;
            }
        }

        public virtual void Union<T> (ref IQueryable<T> a, IQueryable<T> b) {
            if (a != null && b != null) {
                a = a.Union (b);
                return;
            }
            if (b != null)
                a = b;
        }

        public virtual void Intersect<T> (ref IQueryable<T> a, IQueryable<T> b) {
            if (a != null && b != null) {
                a = a.Intersect (b);
                return;
            }
            if (b != null)
                a = b;
        }

        public virtual void Except<T> (ref IQueryable<T> a, IQueryable<T> b) {
            if (a != null && b != null) {
                a = a.Except (b);
                return;
            }
            if (b != null)
                a = b;
        }

        public static IQueryable<T> EmptyQuerable<T> () => new T[0].AsQueryable<T> ();

        public string AsString<T> (T item) => item == null ? "null" : item.ToString ();

        public virtual Expression<Func<E, bool>> ConvertPredicate<I, E> (Expression<Func<I, bool>> source) where E : I
            => ExpressionChangerVisit.Change<I, E> (source) as Expression<Func<E, bool>>;

        public IQueryable<E> WhereIfConvertF<I, E> (IQueryable<E> q, Expression<Func<I, bool>> w) where E : I => q.WhereIf (ConvertPredicate<I, E> (w));

        public virtual bool IsPaging (Paging paging) => paging != null && paging.DataRequired && paging.Count > paging.Limit;

        public virtual IQueryable<T> AddPagingToQuery<T> (IQueryable<T> query, Paging paging) {

            if (!IsPaging (paging)) return query;

            var take = paging.Skip + paging.Take > paging.Count ? paging.Count - paging.Skip : paging.Take;
            return query.Skip (paging.Skip)?.Take (take);
        }

        public virtual void ComposeMainQueryPaging<T> (ref IQueryable<T> query, Paging paging) {
            if (query == null)
                return;
            query = query.Distinct ();

            if (paging != null && paging.CountRequired) {
                paging.Count = query.Count ();
            }

            query = AddPagingToQuery (query, paging);

        }

        public virtual void ComposeMainQueryPaging<T> (QuerablesBase querables) {
            var paging = querables.Paging;
            var queryProp = querables.GetType ()
                .GetProperties ()
                .First (p => typeof (IQueryable<T>).IsAssignableFrom (p.PropertyType));

            var query = queryProp.GetValue (querables, null) as IQueryable<T>;

            ComposeMainQueryPaging (ref query, paging);

            queryProp.SetValue (querables, query, null);
        }

        public (IQueryable<E> entities, IQueryable<R> relations) EntityRelationJoin<E, R> (QueryPredicates preds, IQueryable<E> entities, IQueryable<R> relations, Expression<Func<R, Guid>> key) where E : IIdEntity where R : IIdEntity {

            var entityType = new TypeInfo { Type = typeof (E) };
            var relationType = new TypeInfo { Type = typeof (R) };
            var entity = preds.Resolve.FlagOf (entityType.ImplName);
            var relation = preds.Resolve.FlagOf (relationType.ImplName);

            var entityPred = preds.GetType ().GetProperties ().First (p => p.Name == entityType.GetPlural (entityType.ImplName)).GetValue (preds);
            var relationPred = preds.GetType ().GetProperties ().First (p => p.Name == relationType.GetPlural (relationType.ImplName)).GetValue (preds);

            if ((preds.MainQuery == entity || entityPred != null) && preds.Resolve.HasFlag (relation)) {

                relations = entities.Join (relations, o => o.Id, key, (o, i) => i);

            } else if (preds.MainQuery == relation && preds.Resolve.HasFlag (entity)) {

                entities = entities.Join (relations, o => o.Id, key, (o, i) => o);
            }
            return (entities, relations);
        }

        public (IQueryable<E> entities, IQueryable<R> relations) EntityRelationJoin<E, R> (QueryPredicates preds, IQueryable<E> entities, IQueryable<R> relations, Expression<Func<E, Guid>> key) where E : IIdEntity where R : IIdEntity {

            var entityType = new TypeInfo { Type = typeof (E) };
            var relationType = new TypeInfo { Type = typeof (R) };
            var entity = preds.Resolve.FlagOf (entityType.ImplName);
            var relation = preds.Resolve.FlagOf (relationType.ImplName);

            var entityPred = preds.GetType ().GetProperties ().First (p => p.Name == entityType.GetPlural (entityType.ImplName)).GetValue (preds);
            var relationPred = preds.GetType ().GetProperties ().First (p => p.Name == relationType.GetPlural (relationType.ImplName)).GetValue (preds);

            if ((preds.MainQuery == entity || entityPred != null) && preds.Resolve.HasFlag (relation)) {

                relations = entities.Join (relations, key, o => o.Id, (o, i) => i);

            } else if (preds.MainQuery == relation && preds.Resolve.HasFlag (entity)) {

                entities = entities.Join (relations, key, o => o.Id, (o, i) => o);
            }
            return (entities, relations);
        }

        public Func<Type, Guid> ModelGuid { get; set; }

        public Action<Guid, Guid, RepositoryChangeAction> EntityChanging { get; set; }

        public Func<Guid, bool> CanChangeEntity { get; set; }

        public virtual int RemoveEntities<Q, T> (Q q, IEnumerable<T> entities, Guid userId) where T : IIdEntity where Q : IEntityQuore {
            var count = entities?.Count () ?? -1;
            if (count > 0) {
                Log.Info ($"{nameof (RemoveEntities)}<{typeof (T).Name}> {count}");
                var delDate = DateTime.Now;
                var deleted = new HashSet<IDeletedEntity> ();

                entities = entities.Where (e => CanChangeEntity?.Invoke (e.Id) ?? true);
                var modelGuid = ModelGuid?.Invoke (typeof (T)) ?? Guid.Empty;
                entities.ForEach (e => {
                    EntityChanging?.Invoke (e.Id, userId, RepositoryChangeAction.Removed);
                    deleted.Add (new Model.Dto.DeletedEntity { Id = e.Id, CreatedAt = e.CreatedAt, DeletedAt = delDate, UpdatedAt = e.UpdatedAt, ModelType = modelGuid });
                });
                q.Remove (entities);
                q.Upsert (deleted);
            } else {
                //Log.Debug ($" {count} {nameof (Remove)}<{typeof (T).Name}>");
            }
            return count;
        }

        public virtual int UpsertEntities<Q, T> (Q q, IEnumerable<T> entities, Guid userId) where T : IIdEntity where Q : IEntityQuore {
            var count = entities?.Count () ?? -1;
            if (count > 0) {
                entities = entities.Where (e => CanChangeEntity?.Invoke (e.Id) ?? true);
                Log.Info ($"{nameof (UpsertEntities)}<{typeof (T).Name}> {count}");
                entities.ForEach (e => EntityChanging?.Invoke (e.Id, userId, RepositoryChangeAction.Upserted));
                q.Upsert (entities);
            } else {
                //Log.Debug ($"{count} {nameof (Upsert)}<{typeof (T).Name}>");
            }
            return count;
        }

    }

}
