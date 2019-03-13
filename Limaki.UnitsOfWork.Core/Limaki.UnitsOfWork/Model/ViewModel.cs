using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Limaki.Common;

namespace Limaki.UnitsOfWork {

    public interface IViewModel : ICheckable {

        Store Store { get; set; }
        bool ReadOnly { get; set; }
        string ToString (string format);

    }

    public interface IViewModel<T> : IViewModel {

        T Entity { get; }
    }

    public interface ICheckable {
        string Check ();
    }

    public class ViewModel : IViewModel, IDataErrorInfo {

        public virtual bool IsDirty { get; protected set; }

        public virtual void ClearState () => IsDirty = false;

        Store _store = null;
        public virtual Store Store { get => _store ?? (_store = new Store ()); set => _store = value; }

        public virtual bool ReadOnly { get; set; }

        #region IDataErrorInfo Member

        string IDataErrorInfo.Error => "";

        protected virtual string CheckMember (string value, string memberName, bool required, int? length) {
            if (value != null) {
                value = value.Trim ();
                if (length.HasValue && value.Length > length.Value)
                    return string.Format ("{0} ist zu lang (maximal {1} Zeichen erlaubt)", memberName, length.Value);
            }

            if (required && (value == null || value.Length == 0))
                return memberName + " ist ein Pflichtfeld!";
            return "";
        }

        protected virtual string CheckMember (string value, string memberName, int length) => CheckMember (value, memberName, true, length);

        protected virtual string CheckMember (string value, string memberName) => CheckMember (value, memberName, true, null);

        protected virtual string DataErrorInfo (string memberName) => "";

        string IDataErrorInfo.this[string memberName] { get => DataErrorInfo (memberName); }

        public virtual string Check () {
            var result = new StringBuilder ();
            foreach (var prop in this.GetType ().GetProperties ()) {
                if (prop.CanWrite && prop.GetGetMethod ().IsPublic) {
                    var errorInfo = this as IDataErrorInfo;
                    var errorMsg = errorInfo[prop.Name];
                    if (!string.IsNullOrEmpty (errorMsg)) {
                        result.Append (errorMsg);
                        result.Append ("\n");
                    }
                }
            }

            return result.ToString ();
        }

        #endregion

        /// <summary>
        /// separator
        /// </summary>
        public const string S = " | ";

        /// <summary>
        /// string for null-values
        /// </summary>
        public const string N = "…";

        public virtual string ToString (string format) => ToString ();
    }

    public class ViewModel<T> : ViewModel, IViewModel<T> {

        public ViewModel (T entity) {
            Entity = entity;
        }

        public T Entity { get; protected set; }

        protected string Adjust<V> (string value) => string.IsNullOrEmpty (value) ? default (V).ToString () : value;

        public virtual bool EntityChanged<V> (Action<T, V> setter, V oldValue, V value, string member = null)
        => EntityChanged<T, V> (Entity, setter, oldValue, value, member);

        public virtual bool EntityChanged (Action<T, decimal> setter, decimal oldValue, string value, string member = null)
            => EntityChanged (Entity, setter, oldValue, value, member);
        public virtual bool EntityChanged (Action<T, double> setter, double oldValue, string value, string member = null)
        => EntityChanged (Entity, setter, oldValue, value, member);

        public virtual bool EntityChanged (Action<T, int> setter, int oldValue, string value, string member = null)
        => EntityChanged (Entity, setter, oldValue, value, member);

        public virtual bool EntityChanged (Action<T, long> setter, int oldValue, string value, string member = null)
        => EntityChanged (Entity, setter, oldValue, value, member);

        public virtual bool EntityChanged (Action<T, DateTime> setter, DateTime oldValue, string value, string member = null)
        => EntityChanged (Entity, setter, oldValue, value, member);

        public virtual bool EntityChangedEnum<E> (Action<T, E> setter, E oldValue, string value, string member = null) where E : struct, System.Enum
        => EntityChangedEnum (Entity, setter, oldValue, value, member);

        public virtual bool EntityChanged<A> (A entity, Action<A, decimal> setter, decimal oldValue, string value, string member = null)
        => decimal.TryParse (Adjust<decimal> (value), out var dValue) && EntityChanged (entity, setter, oldValue, dValue, member);

        public virtual bool EntityChanged<A> (A entity, Action<A, double> setter, double oldValue, string value, string member = null)
        => double.TryParse (Adjust<double> (value), out var dValue) && EntityChanged (entity, setter, oldValue, dValue, member);

        public virtual bool EntityChanged<A> (A entity, Action<A, int> setter, int oldValue, string value, string member = null)
        => int.TryParse (Adjust<int> (value), out var dValue) && EntityChanged (entity, setter, oldValue, dValue, member);

        public virtual bool EntityChanged<A> (A entity, Action<A,long> setter, int oldValue, string value, string member = null)
        => long.TryParse (Adjust<long> (value), out var dValue) && EntityChanged (entity, setter, oldValue, dValue, member);

        public virtual bool EntityChanged<A> (A entity, Action<A, DateTime> setter, DateTime oldValue, string value, string member = null)
        => DateTime.TryParse (Adjust<DateTime> (value), out var dValue) && EntityChanged (entity, setter, oldValue, dValue, member);

        public virtual bool EntityChangedEnum<A, E> (A entity, Action<A, E> setter, E oldValue, string value, string member = null) where E : struct, System.Enum
            => Enum.TryParse (value, out E dValue) && EntityChanged (entity, setter, oldValue, dValue, member);

        protected virtual void Update<A> (A entity) => Store.Update (entity);

        public virtual bool EntityChanged<A, V> (A entity, Action<A, V> setter, V oldValue, V newValue, string member = null) {
            var result = !object.Equals (newValue, oldValue);
            if (member == null && result) {
                setter (entity, newValue);
                return result;
            }
            
            if (result && !ReadOnly && Store.IsValidChange (member, entity, oldValue, newValue)) {
                IsDirty = true;
                setter (entity, newValue);
                Update (entity);               
                Store.MemberChanged (member, entity, oldValue, newValue);
                return true;
            }
            return false;
        }

        public virtual void EntityChanged() {
            IsDirty = true;
            Update(Entity);
        }

        protected virtual void EntityCreated<V> (V value) {
            if (!ReadOnly) {
                Store.AddCreated<V> (value);
            }
        }

        protected virtual void EntityRemoved<V> (V value) {
            if (!ReadOnly) {
                Store.Remove (value);
            }
        }

        public virtual T CloneEntity () {
            var result = Store.Create<T> ();
            Guid id = default (Guid);

            var idEntity = result as IIdEntity;
            if (idEntity != null) {
                id = idEntity.Id;
            }

            new Copier<T> ().Copy (Entity, result);

            if (idEntity != null) {
                idEntity.Id = id;
            }
            return result;
        }

        public virtual void ChildsAddingNew<C> (object sender, AddingNewEventArgs e) {
            if (this.Store != null && this.Store.ItemFactory != null && this.Store.ItemFactory.Contains<C> ()) {
                e.NewObject = this.Store.ItemFactory.Create<C> ();
            }
        }

    }
}