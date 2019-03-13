using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Limaki.Common.Collections;
using Limaki.Common.Linqish;
using Limaki.UnitsOfWork.Model;

namespace Limaki.UnitsOfWork {

    public class EntityInteractor {

        public virtual Guid RelationId<T> (Guid id, T value) where T : IIdEntity => value?.Id == null ? id : value.Id;
        public virtual Guid RelationId<T> (T value) where T : IIdEntity => value?.Id == null ? Guid.Empty : value.Id;

        public virtual IEnumerable<T> Relations<T> (IdentityMap map, Func<T, Guid> member, Guid id) => map.Stored<T> ().Where (e => member (e) == id).ToList ();

        public virtual ICollection<T> CreateRelation<T> () => new List<T> ();

        public virtual bool CanChange<T> (IEnumerable<T> relations) => (relations is ICollection<T> collection) && !collection.IsReadOnly;

        public virtual IEnumerable<T> AddToRelation<T> (IEnumerable<T> relations, T item) {

            if (relations is ICollection<T> collection && !collection.IsReadOnly) {
                if (item != null)
                    collection.Add (item);
                return collection;
            }

            if (relations == null) {
                var newlist = CreateRelation<T> ();
                if (item != null)
                    newlist.Add (item);
                return newlist;
            }

            throw new ArgumentException ($"{nameof (DomainOrganizer)}.{nameof (AddToRelation)} does not support {relations?.GetType ()}");
        }

        public virtual IEnumerable<T> RemoveFromRelation<T> (IEnumerable<T> relations, T item) {

            if (relations is ICollection<T> collection && !collection.IsReadOnly) {
                collection.Remove (item);
                return collection;
            }

            throw new ArgumentException ($"{nameof (DomainOrganizer)}.{nameof (AddToRelation)} does not support {relations?.GetType ()}");
        }

        public virtual void SetRelations (IdentityMap map) { }
        public virtual bool GotRelations (ListContainer container) => false;
        public virtual void SetRelationIds (IdentityMap map) { }

        public virtual void SetRelationId<E, M> (E entity, Expression<Func<E, M>> member) where M : IIdEntity {
            
            var amember = new EntityMember<E, M> (member).GetMember (entity);
            var prop = typeof (E).GetProperty($"{(member.Body as MemberExpression).Member.Name}Id");
            var param = Expression.Variable (typeof (E), "entity");
            var memberId = Expression.Lambda<Func<E, Guid>> (Expression.Property (param, prop), param);
            new EntityMember<E, Guid> (memberId).SetMember (entity, amember?.Id ?? Guid.Empty);
        }

        public virtual void SetRelation<E, M> (E entity, Expression<Func<E, M>> member, M value) where M : IIdEntity { 
            new EntityMember<E, M> (member).SetMember (entity,value);
            SetRelationId (entity, member);
        }

        public virtual ILock GetLock<E> (E entity) where E : IIdEntity => new Model.Dto.Lock { Key = entity.Id, MachineName = Environment.MachineName , UserName = Environment.UserName };

    }

    public class EntityInteractor<E> : EntityInteractor {

        public override void SetRelations (IdentityMap map) => map.Stored<E> ().ForEach (e => SetRelations (e, map));
        public override void SetRelationIds (IdentityMap map) => map.Stored<E> ().ForEach (SetRelationIds);
        public override bool GotRelations (ListContainer container) => container.List<E> ().Any ();

        public virtual void SetRelationIds (E entity) { }

        public virtual void SetRelations (E entity, IdentityMap map) { }

        public virtual void SetRelations (IEnumerable<E> items, IdentityMap map) => items.ForEach (i => SetRelations (i, map));

        public virtual void Evict (IEnumerable<E> entities, IdentityMap map) => entities.ForEach (map.Remove);

        public virtual (bool can, FormattableString message) CanRemove (E entity) => (true, null);

        public virtual (bool can, FormattableString message) CanRemove (Store store, E entity) => CanRemove (entity);

        public virtual (bool can, FormattableString message) CanAdd (E entity) {
            if (entity == null)
                return (false, null);

            return (true, null);
        }

        public virtual E Create (Store store) => store.AddCreated (store.Create<E> ());

		public virtual void Remove (Store store, E entity) => store.Remove (entity);

		public virtual bool IsEmpty (E entity) => false;

    }

    public class EntityMember<E, M>  {

        public EntityMember (Expression<Func<E, M>> member) {
            MemberExpression = member;
            MemberName = (MemberExpression.Body as MemberExpression).Member.Name;
            if (!getters.TryGetValue (MemberName, out var _getMember)){
                _getMember = member.Compile ();
                getters[MemberName] = _getMember;
            }
            GetMember = _getMember;
            if (!setters.TryGetValue (MemberName, out var _setMember)){
                var paras = new ParameterExpression[] { Expression.Variable (typeof (E), "item"), Expression.Variable (typeof (M), "member") };
                var setterExpression = Expression.Lambda (
                    typeof (Action<E, M>),
                    Expression.Assign (Expression.Property (paras[0], (MemberExpression.Body as MemberExpression).Member as PropertyInfo), paras[1]),
                    paras);

                _setMember = (Action<E, M>)setterExpression.Compile ();
            }
            SetMember = _setMember;
        }

        static Dictionary<string, Func<E, M>> getters = new Dictionary<string, Func<E, M>> ();
        static Dictionary<string, Action<E, M>> setters = new Dictionary<string, Action<E, M>> ();

        public Expression<Func<E, M>> MemberExpression { get; protected set; }

        public Func<E, M> GetMember { get; protected set; }
        public Action<E, M> SetMember { get; protected set; }

        public string MemberName { get; protected set; }

    }
}