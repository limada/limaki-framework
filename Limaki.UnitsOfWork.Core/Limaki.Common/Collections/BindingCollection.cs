// Authors:
//    Zoltan Varga (vargaz@gmail.com)
//    David Waite (mass@akuma.org)
//    Marek Safar (marek.safar@gmail.com)
//
// (C) 2005 Novell, Inc.
// (C) 2005 David Waite
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc.
//

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Limaki.Common.Collections {
    /// <summary>
    /// BindingList-Implementation from Mono
    /// https://github.com/mono/mono/blob/master/mcs/class/System/System.ComponentModel/BindingList.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindingCollection<T> : CollectionBase<T>,
                                        IBindingList, IList, ICollection,
                                        IEnumerable, ICancelAddNew, IRaiseItemChangedEvents {

        public BindingCollection(IList<T> list)
            : base(list) {
            CheckType();
        }

        public BindingCollection()
            : base() {
            CheckType();
        }

        bool allow_edit = true;
        bool allow_remove = true;
        bool allow_new;
        bool allow_new_set;

        bool raise_list_changed_events = true;

        bool type_has_default_ctor;
        bool type_raises_item_changed_events;

        bool add_pending;
        int pending_add_index;

        void CheckType() {
            var ci = typeof(T).GetConstructor(Type.EmptyTypes);
            type_has_default_ctor = (ci != null);
            type_raises_item_changed_events = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
        }

        public bool AllowEdit {
            get { return allow_edit; }
            set {
                if (allow_edit != value) {
                    allow_edit = value;

                    if (raise_list_changed_events)
                        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1 /* XXX */));
                }
            }
        }

        public bool AllowNew {
            get {
                /* if the user explicitly it, return that value */
                if (allow_new_set)
                    return allow_new;

                /* if the list type has a default constructor we allow new */
                if (type_has_default_ctor)
                    return true;

                /* if the user adds a delegate, we return true even if
                   the type doesn't have a default ctor */
                if (AddingNew != null)
                    return true;

                return false;
            }
            set {
                // this funky check (using AllowNew
                // instead of allow_new allows us to
                // keep the logic for the 3 cases in
                // one place (the getter) instead of
                // spreading them around the file (in
                // the ctor, in the AddingNew add
                // handler, etc.
                if (AllowNew != value) {
                    allow_new_set = true;

                    allow_new = value;

                    if (raise_list_changed_events)
                        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1 /* XXX */));
                }
            }
        }

        public bool AllowRemove {
            get { return allow_remove; }
            set {
                if (allow_remove != value) {
                    allow_remove = value;

                    if (raise_list_changed_events)
                        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1 /* XXX */));
                }
            }
        }

        protected virtual bool IsSortedCore {
            get { return false; }
        }

        public bool RaiseListChangedEvents {
            get { return raise_list_changed_events; }
            set { raise_list_changed_events = value; }
        }

        protected virtual ListSortDirection SortDirectionCore {
            get { return ListSortDirection.Ascending; }
        }

        protected virtual PropertyDescriptor SortPropertyCore {
            get { return null; }
        }

        protected virtual bool SupportsChangeNotificationCore {
            get { return true; }
        }

        protected virtual bool SupportsSearchingCore {
            get { return false; }
        }

        protected virtual bool SupportsSortingCore {
            get { return false; }
        }

        public event AddingNewEventHandler AddingNew;
        public event ListChangedEventHandler ListChanged;

        public T AddNew() {
            return (T)AddNewCore();
        }

        protected virtual object AddNewCore() {
            if (!AllowNew)
                throw new InvalidOperationException();

            AddingNewEventArgs args = new AddingNewEventArgs();

            OnAddingNew(args);

            T new_obj = (T)args.NewObject;
            if (new_obj == null) {
                if (!type_has_default_ctor)
                    throw new InvalidOperationException();

                new_obj = (T)Activator.CreateInstance(typeof(T));
            }

            Add(new_obj);
            pending_add_index = IndexOf(new_obj);
            add_pending = true;

            return new_obj;
        }

        protected virtual void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
            throw new NotSupportedException();
        }

        public virtual void CancelNew(int itemIndex) {
            if (!add_pending)
                return;

            if (itemIndex != pending_add_index)
                return;

            add_pending = false;

            base.RemoveItem(itemIndex);

            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, itemIndex));
        }

        protected override void ClearItems() {
            EndNew(pending_add_index);
            if (type_raises_item_changed_events) {
                foreach (T item in base.Items) {
                    (item as INotifyPropertyChanged).PropertyChanged -= Item_PropertyChanged;
                }
            }
            base.ClearItems();
            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public virtual void EndNew(int itemIndex) {
            if (!add_pending)
                return;

            if (itemIndex != pending_add_index)
                return;

            add_pending = false;
        }

        protected virtual int FindCore(PropertyDescriptor prop, object key) {
            throw new NotSupportedException();
        }

        protected override void InsertItem(int index, T item) {
            EndNew(pending_add_index);

            base.InsertItem(index, item);

            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));

            if (type_raises_item_changed_events)
                (item as INotifyPropertyChanged).PropertyChanged += Item_PropertyChanged;
        }

        void Item_PropertyChanged(object item, PropertyChangedEventArgs args) {
            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, base.IndexOf((T)item)));
        }

        protected virtual void OnAddingNew(AddingNewEventArgs e) {
            AddingNew?.Invoke(this, e);
        }

        protected virtual void OnListChanged(ListChangedEventArgs e) {
            ListChanged?.Invoke(this, e);
        }

        protected override void RemoveItem(int index) {
            if (!AllowRemove)
                throw new NotSupportedException();

            EndNew(pending_add_index);
            if (type_raises_item_changed_events) {
                (base[index] as INotifyPropertyChanged).PropertyChanged -= Item_PropertyChanged;
            }
            base.RemoveItem(index);

            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        protected virtual void RemoveSortCore() {
            throw new NotSupportedException();
        }

        public void ResetBindings() {
            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public void ResetItem(int position) {
            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, position));
        }

        protected override void SetItem(int index, T item) {
            if (type_raises_item_changed_events) {
                (base[index] as INotifyPropertyChanged).PropertyChanged -= Item_PropertyChanged;
                (item as INotifyPropertyChanged).PropertyChanged += Item_PropertyChanged;
            }
            base.SetItem(index, item);
            if (raise_list_changed_events)
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
        }

        void IBindingList.AddIndex(PropertyDescriptor index) {
            /* no implementation by default */
        }

        object IBindingList.AddNew() {
            return AddNew();
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) {
            ApplySortCore(property, direction);
        }

        int IBindingList.Find(PropertyDescriptor property, object key) {
            return FindCore(property, key);
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property) {
            /* no implementation by default */
        }

        void IBindingList.RemoveSort() {
            RemoveSortCore();
        }

        bool IBindingList.IsSorted {
            get { return IsSortedCore; }
        }

        ListSortDirection IBindingList.SortDirection {
            get { return SortDirectionCore; }
        }

        PropertyDescriptor IBindingList.SortProperty {
            get { return SortPropertyCore; }
        }

        bool IBindingList.AllowEdit {
            get { return AllowEdit; }
        }

        bool IBindingList.AllowNew {
            get { return AllowNew; }
        }

        bool IBindingList.AllowRemove {
            get { return AllowRemove; }
        }

        bool IBindingList.SupportsChangeNotification {
            get { return SupportsChangeNotificationCore; }
        }

        bool IBindingList.SupportsSearching {
            get { return SupportsSearchingCore; }
        }

        bool IBindingList.SupportsSorting {
            get { return SupportsSortingCore; }
        }

        bool IRaiseItemChangedEvents.RaisesItemChangedEvents {
            get { return type_raises_item_changed_events; }
        }
    }

    public interface INestedCollection<T> {
        IEnumerable<T> Inner { get; }
    }

    public interface INestedCollection {
        IEnumerable Inner { get; }
    }

    /// <summary>
    /// Collection-Implementation from Mono
    /// https://github.com/mono/mono/blob/master/mcs/class/Corlib/System.Collections.ObjectModel/Collection.cs
    /// 
    /// <typeparam name="T"></typeparam>
    [ComVisible(false)]
    [Serializable]
    [DebuggerDisplay("Count={Count}")]
    public class CollectionBase<T> : IList<T>, IList, INestedCollection<T>, INestedCollection
#if NET_4_5
		, IReadOnlyList<T>
#endif
    {
        IList<T> list;
        object syncRoot;

        public CollectionBase() {
            List<T> l = new List<T>();
            IList l2 = l as IList;
            syncRoot = l2.SyncRoot;
            list = l;
        }

        public CollectionBase(IList<T> list) {
            if (list == null)
                throw new ArgumentNullException("list");
            this.list = list;
            ICollection l = list as ICollection;
            syncRoot = (l != null) ? l.SyncRoot : new object();
        }

        public void Add(T item) {
            int idx = list.Count;
            InsertItem(idx, item);
        }

        public void Clear() {
            ClearItems();
        }

        protected virtual void ClearItems() {
            list.Clear();
        }

        public bool Contains(T item) {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int index) {
            list.CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator() {
            return list.GetEnumerator();
        }

        public int IndexOf(T item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            InsertItem(index, item);
        }

        protected virtual void InsertItem(int index, T item) {
            list.Insert(index, item);
        }

        protected virtual IList<T> Items {
            get { return list; }
            set { list = value;}
        }

        public bool Remove(T item) {
            int idx = IndexOf(item);
            if (idx == -1)
                return false;

            RemoveItem(idx);

            return true;
        }

        public void RemoveAt(int index) {
            RemoveItem(index);
        }

        protected virtual void RemoveItem(int index) {
            list.RemoveAt(index);
        }

        public int Count {
            get { return list.Count; }
        }

        public T this[int index] {
            get { return list[index]; }
            set { SetItem(index, value); }
        }

        bool ICollection<T>.IsReadOnly {
            get { return list.IsReadOnly; }
        }

        protected virtual void SetItem(int index, T item) {
            list[index] = item;
        }


        #region Helper methods for non-generic interfaces

        internal static bool IsValidItem(object item) {
            return (item is T || (item == null && !typeof(T).IsValueType));
        }

        internal static T ConvertItem(object item) {
            if (IsValidItem(item))
                return (T)item;
            throw new ArgumentException("item");
        }

        internal static void CheckWritable(IList<T> list) {
            if (list.IsReadOnly)
                throw new NotSupportedException();
        }

        internal static bool IsSynchronized(IList<T> list) {
            ICollection c = list as ICollection;
            return (c != null) ? c.IsSynchronized : false;
        }

        internal static bool IsFixedSize(IList<T> list) {
            IList l = list as IList;
            return (l != null) ? l.IsFixedSize : false;
        }
        #endregion

        #region Not generic interface implementations
        void ICollection.CopyTo(Array array, int index) {
            ((ICollection)list).CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)list.GetEnumerator();
        }

        int IList.Add(object value) {
            int idx = list.Count;
            InsertItem(idx, ConvertItem(value));
            return idx;
        }

        bool IList.Contains(object value) {
            if (IsValidItem(value))
                return list.Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value) {
            if (IsValidItem(value))
                return list.IndexOf((T)value);
            return -1;
        }

        void IList.Insert(int index, object value) {
            InsertItem(index, ConvertItem(value));
        }

        void IList.Remove(object value) {
            CheckWritable(list);

            int idx = IndexOf(ConvertItem(value));

            RemoveItem(idx);
        }

        bool ICollection.IsSynchronized {
            get { return IsSynchronized(list); }
        }

        object ICollection.SyncRoot {
            get { return syncRoot; }
        }
        bool IList.IsFixedSize {
            get { return IsFixedSize(list); }
        }

        bool IList.IsReadOnly {
            get { return list.IsReadOnly; }
        }

        object IList.this[int index] {
            get { return list[index]; }
            set { SetItem(index, ConvertItem(value)); }
        }
        #endregion

        IEnumerable<T> INestedCollection<T>.Inner { get { return this.Items; } }

        IEnumerable INestedCollection.Inner { get { return this.Items; } }
    }
}