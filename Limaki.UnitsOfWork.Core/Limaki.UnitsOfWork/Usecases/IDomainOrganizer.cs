using System;
using System.Collections.Generic;

namespace Limaki.UnitsOfWork {
    
    public interface IDomainOrganizer {
        
        IEnumerable<T> AddToRelation<T> (IEnumerable<T> relations, T item);
        bool CanChange<T> (IEnumerable<T> relations);
        ICollection<T> CreateRelation<T> ();
        bool GotRelations (ListContainer container);
        Guid RelationId<T> (Guid id, T value) where T : IIdEntity;
        IEnumerable<T> Relations<T> (IdentityMap map, Func<T, Guid> member, Guid id);
        IEnumerable<T> RemoveFromRelation<T> (IEnumerable<T> relations, T item);
        void SetRelationIds (IdentityMap map);
        void SetRelations (IdentityMap map);

        bool IsEmpty<T> (T entity);
        void SaveChanges ();
    }
}