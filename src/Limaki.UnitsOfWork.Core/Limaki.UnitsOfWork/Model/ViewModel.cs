/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Limaki.Common;
using Limaki.UnitsOfWork.IdEntity.Model;

namespace Limaki.UnitsOfWork {

    public interface IJsonPagedInput {

        string PagingJson { get; set; }

    }

    public interface ICheckable {

        string Check ();

    }

    public class ViewModel : IViewModel, IDataErrorInfo {

        public virtual bool IsDirty { get; protected set; }

        public virtual void ClearState () => IsDirty = false;

        Store _store = null;

        public virtual Store Store { get => _store ??= new Store (); set => _store = value; }

        public virtual bool ReadOnly { get; set; }

        #region IDataErrorInfo Member

        string IDataErrorInfo.Error => "";

        protected virtual string CheckMember (string value, string memberName, bool required, int? length) {
            if (value != null) {
                value = value.Trim ();

                if (length.HasValue && value.Length > length.Value)
                    return $"{memberName} ist zu lang (maximal {length.Value} Zeichen erlaubt)";
            }

            if (required && string.IsNullOrEmpty (value))
                return memberName + " ist ein Pflichtfeld!";

            return "";
        }

        protected virtual string CheckMember (string value, string memberName, int length) => CheckMember (value, memberName, true, length);

        protected virtual string CheckMember (string value, string memberName) => CheckMember (value, memberName, true, default);

        protected virtual string DataErrorInfo (string memberName) => "";

        string IDataErrorInfo.this [string memberName] => DataErrorInfo (memberName);

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

        public ViewModel (T entity) { Entity = entity; }

        public T Entity { get; protected set; }

        protected string Adjust<V> (string value) => string.IsNullOrEmpty (value) ? default(V).ToString () : value;

        public virtual bool PropertyChanged<V> (Action<T, V> setter, V oldValue, V value, [CallerMemberName] string member = null)
            => PropertyChanged<T, V> (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, decimal> setter, decimal oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, double> setter, double oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, int> setter, int oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, long> setter, long oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, DateTime> setter, DateTime oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, DateTime?> setter, DateTime? oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, short> setter, short oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChangedEnum<E> (Action<T, E> setter, E oldValue, string value, [CallerMemberName] string member = null) where E : struct, System.Enum
            => PropertyChangedEnum (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, decimal> setter, decimal oldValue, string value, [CallerMemberName] string member = null)
            => decimal.TryParse (Adjust<decimal> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChanged (Action<T, char> setter, char oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged (Action<T, char?> setter, char? oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (Entity, setter, oldValue, value, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, double> setter, double oldValue, string value, [CallerMemberName] string member = null)
            => double.TryParse (Adjust<double> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, int> setter, int oldValue, string value, [CallerMemberName] string member = null)
            => int.TryParse (Adjust<int> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, long> setter, long oldValue, string value, [CallerMemberName] string member = null)
            => long.TryParse (Adjust<long> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, DateTime> setter, DateTime oldValue, string value, [CallerMemberName] string member = null)
            => DateTime.TryParse (Adjust<DateTime> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, short> setter, short oldValue, string value, [CallerMemberName] string member = null)
            => short.TryParse (Adjust<short> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChangedEnum<A, E> (A entity, Action<A, E> setter, E oldValue, string value, [CallerMemberName] string member = null) where E : struct, System.Enum
            => Enum.TryParse (value, out E dValue) && PropertyChanged (entity, setter, oldValue, dValue, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, DateTime?> setter, DateTime? oldValue, string value, [CallerMemberName] string member = null)
            => DateTime.TryParse (Adjust<DateTime> (value), out var dValue) && PropertyChanged (entity, setter, oldValue, string.IsNullOrEmpty (value) ? default(DateTime?) : dValue, member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, char> setter, char oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (entity, setter, oldValue, string.IsNullOrEmpty (value) ? default(char) : value[0], member);

        public virtual bool PropertyChanged<A> (A entity, Action<A, char?> setter, char? oldValue, string value, [CallerMemberName] string member = null)
            => PropertyChanged (entity, setter, oldValue, value == null ? default(char?) : (string.IsNullOrEmpty (value) ? default(char) : value[0]), member);

        protected virtual void Update<A> (A entity) => Store.Update (entity);

        public virtual bool PropertyChanged<A, V> (A entity, Action<A, V> setter, V oldValue, V newValue, [CallerMemberName] string member = null) {
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

        public virtual void EntityChanged () {
            IsDirty = true;
            Update (Entity);
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
            Guid id = default(Guid);

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