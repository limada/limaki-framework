/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Limaki.Common;
using System.Text.RegularExpressions;

namespace Limaki.Common {
    /// <summary>
    /// A Trie-Tree based query class
    /// search results are cached in the tree
    /// Find calls the QueryItems-delegate, if needsQuery == true
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrieQuery<T> {

        public TrieQuery () {
            this.Threshold = 0;
        }

        TrieTree<T> _tree = null;
        public virtual TrieTree<T> Tree => _tree ?? (_tree = new TrieTree<T> (false) { CreateValueList = CreateValueList });

        TrieTree<string> _done = null;
        protected TrieTree<string> Done => _done ?? (_done = new TrieTree<string> (false));

        public virtual void Clear () {
            _tree = null;
            _done = null;
            _failed = null;
        }

        public int Threshold { get; set; }
        public bool Split { get; set; }
        public bool DoFindContains { get; set; }

        public virtual Func<ICollection<T>> CreateValueList { get; set; }
        public virtual Func<T, string> ItemToString { get; set; }
        public virtual Func<string, IEnumerable<T>> Query { get; set; }

        public virtual string DoItemToString (T item) {
            if (ItemToString != null) {
                return ItemToString (item);
            }
            return item.ToString ();
        }

        #region Query-Handling
        ICollection<string> _failed = null;
        protected ICollection<string> Failed => _failed ?? (_failed = new HashSet<string> ());
        protected virtual IEnumerable<T> DoQuery (string key) {
            if (Query != null) {
                return Query (key);
            }
            throw new ArgumentException (this.GetType ().Name + " needs a QueryItems-Delegate");
        }

        public virtual bool NeedsQuery (string key) {
            int len = key.Length;
            while (len > 0) {
                if (Done.ContainsItemsAt (key.Substring (0, len)))
                    return false;
                len--;
            }
            return true;
        }

        public virtual IEnumerable<T> Find (string key) {
            if (string.IsNullOrEmpty (key) && Threshold > 0)
                yield break;

            var needsQuery =
                Query != null &&
                key.Length >= this.Threshold &&
                !Failed.Contains (key) &&
                NeedsQuery (key);
            IEnumerable<T> query = null;
            if (needsQuery) {
                query = DoQuery (key);
                if (query != null) {
                    Done.Add (key, key);
                    if (query.Any()) {
                        AddRange (query);
                    } else {
                        Failed.Add (key);
                    }
                }
            }

            if (string.IsNullOrEmpty (key) && query!=null) {
                foreach (var item in query) {
                    yield return item;
                }
                yield break;
            }

            Tree.DoFindContains = this.DoFindContains;
            foreach (var item in Tree.FindKey (key)) {
                yield return item;
            }
        }

        #endregion


        #region TrieTree-Facade


        public IEnumerable<string> Words (T item) {
            return Words (DoItemToString (item));
        }

        protected static Regex wordex = new Regex (@"\W+", RegexOptions.Compiled);
        public IEnumerable<string> Words (string s) {

            if (string.IsNullOrEmpty (s))
                yield break;

            if (Split) {
                foreach (var w in wordex.Split (s))
                    yield return w;
            }
            yield return s;
        }

        public virtual void Add (T item) {
            if (item != null) {
                foreach (var s in Words (item))
                    Tree.Add (s, item);
            }
        }

        public virtual void AddRange (IEnumerable<T> items) {
            foreach (var item in items) {
                Add (item);
            }
        }

        public virtual void Remove (T item) {
            if (item != null) {
                var s = ItemToString (item);
                foreach (var word in Words (s)) {
                    Tree.Remove (word, item);

                }
                Done.Add (s, s);
            }
        }


        public virtual void Replace (string oldkey, T item) {
            if (item != null) {
                if (!Split) {
                    var s = ItemToString (item);
                    Tree.Replace (oldkey, s, item);
                } else {
                    foreach (var w in Words (oldkey)) {
                        Tree.Remove (w, item);
                    }
                    foreach (var w in Words (item)) {
                        Tree.Add (w, item);
                    }
                }
                Done.Add (oldkey, oldkey);
            }
        }

        public void Replace (T olditem, T newitem) {
            var s = DoItemToString (olditem);
            var s1 = DoItemToString (newitem);
            if (!string.IsNullOrEmpty (s) && s.Equals (s1) && olditem != null) {
                foreach (var w in Words (s)) {
                    Tree.Replace (w, olditem, newitem);
                }
                return;
            }
            if (olditem != null && !string.IsNullOrEmpty (s)) {
                foreach (var w in Words (s)) {
                    Tree.Remove (w, olditem);
                }
                Done.Add (s, s);
            }
            if (newitem != null && !string.IsNullOrEmpty (s1)) {
                foreach (var w in Words (s1)) {
                    Tree.Add (w, newitem);
                }
            }
        }

        #endregion


    }

    public class AutoCompleteEventArgs<T> : EventArgs {
        public AutoCompleteEventArgs (T item) {
            this.Item = item;
        }
        public T Item { get; set; }
        public bool Handled { get; set; }
    }
}