/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Limaki.UnitsOfWork {

    public class PagingCollection<T> : ICollection<T>, IPagingCollection, IDisposable {
        
        Func<Paging, IEnumerable<T>> _pageGetter = null;
        public virtual Func<Paging, IEnumerable<T>> PageGetter {
            get { return _pageGetter; }
            set {
                _pageGetter = value;
                _pages = null;
            }
        }

        /// <summary>
        /// Number of Pages to hold in cache
        /// </summary>
        public int PageCacheCount { get; set; }

        /// <summary>
        /// rowindex
        /// </summary>
        public Action<Paging, int> BeforeGetRow { get; set; }
        /// <summary>
        /// Paging, PageNr
        /// </summary>
        public Action<Paging, int> BeforeGetPage { get; set; }
        /// <summary>
        /// Paging, page-list, PageNr
        /// </summary>
        public Action<Paging, IList<T>, int> AfterGetPage { get; set; }
        /// <summary>
        /// Paging, page-list, PageNr
        /// </summary>
        public Action<Paging, IList<T>, int> BeforeRemovePage { get; set; }

        /// <summary>
        /// page-list, PageNr
        /// </summary>
        public Action<Paging, IList<T>, int> OnPage { get; set; }
        public Action<Paging, T, int> AfterGetRow { get; set; }


        public bool UseThreading { get; set; }

        public SynchronizationContext SyncContext { get; set; }
        public IntPtr SyncHandle { get; set; }

        /// <summary>
        /// gives back null in GetRow() if iPage == lastPage
        /// used to avoid expensive calls to end of table in databases
        /// </summary>
        public bool LastPageAsNullPage { get; set; }

        protected virtual void Housekeeping(int rowindex) {
            if (PageCacheCount == 0)
                PageCacheCount = 1;

            var iPage = (rowindex - (rowindex % PageSize)) / PageSize;
            var lastPage = Count / PageSize;
            var limit = Math.Max(PageSize * PageCacheCount, Paging.Limit);
            var pageLimit = limit / PageSize;
            if (Pages.Count > pageLimit) {
                foreach (var kvp in Pages
                    .Where(page => (page.Key < (iPage - 1) || page.Key > (iPage + 1)) && page.Key != 0 && page.Key != lastPage)
                    .ToArray()) {
                    if (BeforeRemovePage != null) {
                        BeforeRemovePage(this.Paging, kvp.Value, kvp.Key);
                    }
                    Pages.Remove(kvp);
                    //Trace.WriteLine(string.Format("Housekeeping({0}): removed page {1}", rowindex, kvp.Key));
                }
            }
        }

     

        Object lokker = new Object();
        protected int iCalls = 0;

        protected void IncWait() {
            if (SyncContext != null) {
                Interlocked.Exchange(ref iCalls, 1);
                while (SyncContext.IsWaitNotificationRequired())
                    SyncContext.Wait(new IntPtr[] { SyncHandle }, true, 0);
                SyncContext.OperationStarted();
            } else if (UseThreading) {
                while (0 == Interlocked.Exchange(ref iCalls, 1)) {
                    Thread.Sleep(0);
                }
            } else
                iCalls++;
        }

        protected void Dec() {
            if (SyncContext != null) {
                SyncContext.OperationCompleted();
                Interlocked.Exchange(ref iCalls, 0);
            } else if (UseThreading) {
                Interlocked.Exchange(ref iCalls, 0);
            } else {
                iCalls--;
            }
        }

        public virtual T GetRow(int rowindex) {

            lock (lokker) {

                IncWait();
                
                //Trace.WriteLine(string.Format("GetRow ({0}) (calls {1})", rowindex, iCalls));
                var pageNr = (rowindex - (rowindex % PageSize)) / PageSize;

                if (BeforeGetRow != null)
                    BeforeGetRow(Paging, rowindex);

                if (LastPageAsNullPage) {
                    if (rowindex + PageSize >= Count) {
                        Dec();
                        return default(T);
                    }
                }

                IList<T> page = null;

                if (!Pages.TryGetValue(pageNr, out page)) {
                    Paging.Skip = pageNr * PageSize;
                    Paging.Take = PageSize;
                    Paging.DataRequired = true;
                    Paging.CountRequired = Paging.Count == -1;

                    if (BeforeGetPage != null)
                        BeforeGetPage(Paging, pageNr);
                    IEnumerable<T> got = null;
                    if (UseThreading) {
                        if (SyncContext != null) {
                            SyncContext.Send(new SendOrPostCallback(state => PageGetter(Paging)), null);
                        } else {
                            var task = Task.Factory.StartNew(() => PageGetter(Paging));
                            got = task.Result;
                        }
                    } else {
                        got = PageGetter(Paging);
                    }
                    if (got is IList<T>)
                        page = got as IList<T>;
                    else
                        page = got.ToArray();

                    // shield the page against multi tasking destructions
                    var shield = new ObservableCollection<T>(page);
                    shield.CollectionChanged += (s, e) =>
                        Trace.WriteLine(string.Format("ERROR in PagingCollection: Page {0}", e.Action));
                    page = shield;

                    Pages[pageNr] = page;
                    Housekeeping(rowindex);

                    if (AfterGetPage != null)
                        AfterGetPage(Paging, page, pageNr);
                }


                if (OnPage != null)
                    OnPage(Paging, page, pageNr);

                var iRow = (rowindex % PageSize);
                var result = page[iRow];
                if (AfterGetRow != null)
                    AfterGetRow(Paging, result, rowindex);

                
                Dec();
                //if (result != null) 
                //    Trace.WriteLine(string.Format("DONE: GetRow ({0})", rowindex));
                //else
                //    Trace.WriteLine(string.Format("ERROR - DONE: GetRow ({0})", rowindex));
                return result;

            }
        }

        protected IDictionary<int, IList<T>> _pages = null;
        public virtual IDictionary<int, IList<T>> Pages {
            get {
                if (_pages == null) {
                    _pages = new ConcurrentDictionary<int, IList<T>>();
                }
                return _pages;
            }
        }

        protected Paging _paging = null;
        public virtual Paging Paging {
            get { return _paging ?? (_paging = new Paging { Take = 50 }); }
            set { _paging = value; }
        }

        public virtual int PageSize {
            get { return Paging.Take; }
            set { Paging.Take=value; }
        }

        protected int? _count = null;
        public virtual int Count {
            get {
                if (_count == null && PageGetter != null ) {
                    Paging.DataRequired = false;
                    Paging.CountRequired = true;
                    PageGetter(Paging);
                    _count = Paging.Count;
                }
                if (_count == null)
                    _count = 0;
                
                return Paging.Count;

            }
        }

        public virtual void SetCount(int value) {
            _count = value;
        }
        public virtual void Add(T item) {
            throw new NotImplementedException();
        }

        public virtual bool Remove(T item) {
            throw new NotImplementedException();
        }

        public virtual bool Contains(T item) {
            throw new NotImplementedException();
        }

        public virtual void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public virtual bool IsReadOnly {
            get { return true; }
        }

        public virtual IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++)
                yield return GetRow(i);
        }

        public virtual IEnumerable<T> CashedItems() {
           foreach(var page in Pages)
               foreach(var item in page.Value)
                    yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public virtual void Dispose(bool disposing) { }

        public virtual void Dispose() {
            Dispose(true);
        }

        public virtual void Clear() {
            IncWait();
            _count = null;
            Pages.Clear();
            Dec();
        }
        

    }

    public interface IPagingCollection {
        Paging Paging { get; }
    }
}