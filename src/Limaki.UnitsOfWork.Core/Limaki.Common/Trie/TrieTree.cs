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
using System.Text;
using System.Linq;

namespace Limaki.Common {


    public class TrieTree<T> {

        public TrieTree (bool caseSensitive) {
            this.caseSensitive = caseSensitive;

            _root = new TrieBucket<T> ();

        }

        TrieBucket<T> _root;

        bool caseSensitive;

        public Func<ICollection<T>> CreateValueList { get; set; }
        protected ICollection<T> DoCreateValueList () {
            if (CreateValueList == null) {
                return new HashSet<T> ();
            } else {
                return CreateValueList ();
            }
        }

        public TrieBucket<T> Root {
            get { return _root; }
        }

        TrieBucket<T> InsertBucket (TrieBucket<T> q, char c) {
            var bucket = FindSiblingAtState (q.Next, c);
            if (bucket == null) {
                bucket = new TrieBucket<T> ();
                bucket.Value = c;
                bucket.Sibling = q.Next;
                q.Next = bucket;
            }
            return bucket;
        }



        int max_length = 0;
        public void Add (string key, T item) {
            var q = this.Root;
            int depth = 0;

            for (int idx = 0; idx < key.Length; idx++) {
                char c = key[idx];
                if (!caseSensitive)
                    c = Char.ToLower (c);

                q = InsertBucket (q, c);

                depth++;
            }

            if (q.Items == null) {
                q.Items = DoCreateValueList ();
            }
            q.Items.Add (item);


            max_length = Math.Max (max_length, key.Length);
        }

        public bool Remove (string key, T item) {
            var result = false;
            var q = FindBucketStartWith (key);
            while (q != null) {
                if (q.Items != null) {
                    result |= q.Items.Remove (item);
                    if (q.Items.Count == 0) {
                        q.Items = null;
                    }
                }
                if (q.Items == null && q.Next == null && key.Length > 0) {
                    var rootKey = key.Substring (0, key.Length - 1);
                    var keyRoot = FindBucketStartWith (rootKey);
                    var next = keyRoot.Next;

                    if (next != q) {
                        while (next.Sibling != q)
                            next = next.Sibling;
                        next.Sibling = q.Sibling;
                    } else {
                        keyRoot.Next = q.Sibling;
                    }

                    q = keyRoot;
                    key = rootKey;
                } else {
                    break;
                }
            }
            return result;
        }

        public void Replace (string oldkey, string key, T item) {
            Remove (oldkey, item);
            Add (key, item);
        }

        public void Replace (string key, T olditem, T newitem) {
            var q = FindBucketStartWith (key);
            if (q.Items != null) {
                q.Items.Remove (olditem);
                q.Items.Add (newitem);
            }
        }



        public IEnumerable<TrieBucket<T>> FindMatchesAtState (TrieBucket<T> s) {
            if (s == null || s.Next == null)
                yield break;

            var stack = new Stack<TrieBucket<T>> ();
            stack.Push (s.Next);

            while (stack.Count > 0) {
                var m = stack.Pop ();

                if (m != null) {
                    if (m.Items != null) {
                        yield return m;
                    }
                    // could be else?:
                    if (m.Next != null) {
                        stack.Push (m.Next);
                    }
                }
                if (m.Sibling != null) {
                    stack.Push (m.Sibling);
                }

            }

        }

        // Iterate the siblings at state %s looking for the first match
        // containing %c.
        TrieBucket<T> FindSiblingAtState (TrieBucket<T> m, char c) {
            while (m != null && m.Value != c)
                m = m.Sibling;

            return m;
        }

        public TrieBucket<T> FindBucketStartWith (string key) {
            if (string.IsNullOrEmpty (key)) {
                return Root;
            }
            var q = Root.Next;
            int idx = 0;

            while (idx < key.Length) {
                char c = key[idx++];
                if (!caseSensitive)
                    c = Char.ToLower (c);

                q = FindSiblingAtState (q, c);

                if (q != null) {
                    // Got a match!
                    if (idx == key.Length) {
                        return q;
                    } else {
                        q = q.Next;
                    }
                } else {
                    break;
                }
            }
            return null;
        }

        TrieBucket<T> FindContainsAtState (TrieBucket<T> m, char c) {
            if (m == null)
                return m;
            var next = new Stack<TrieBucket<T>> ();
            next.Push (m);
            while (next.Count > 0) {
                m = next.Pop ();
                if (m.Value == c)
                    return m;
                if (m.Sibling != null)
                    next.Push (m.Sibling);
                if (m.Next != null)
                    next.Push (m.Next);
            }
            return null;
        }

        public ICollection<TrieBucket<T>> FindBucketContains (string key) {
            if (string.IsNullOrEmpty (key)) {
                return null;
            }
            var q = Root.Next;
            int idx = 0;

            var next = new Stack<TrieBucket<T>> ();
            var result = new HashSet<TrieBucket<T>> ();
            while (idx < key.Length) {

                char c = key[idx];
                if (!caseSensitive)
                    c = Char.ToLower (c);

                var i = q;
                q = null;
                while (i != null) {
                    if (i.Next != null)
                        next.Push (i.Next);
                    if (i.Value == c) {
                        q = i;
                    }
                    i = i.Sibling;
                }


                if (q != null) {
                    // Got a match!
                    if (idx == (key.Length - 1)) {
                        result.Add (q);
                        q = null;
                    } else {
                        q = q.Next;
                        idx++;
                    }
                }
                if (q == null) {
                    if (next.Count > 0) {
                        q = next.Pop ();
                        idx = 0;
                    } else
                        break;
                }
            }
            if (result.Count == 0)
                return null;
            else
                return result;
        }

        /// <summary>
        /// if true, search with Contains(key), otherwise StartsWith(key)
        /// </summary>
        public bool DoFindContains { get; set; }

        public IEnumerable<T> FindKey (string key) {

            IEnumerable<T> TOf (TrieBucket<T> bucket)
            {
                var q = bucket;
                if (q != null) {
                    var done = new HashSet<T> ();
                    if (q.Items != null)
                        foreach (var item in q.Items) {
                            if (!done.Contains (item)) {
                                done.Add (item);
                                yield return item;
                            }
                        }

                    foreach (var f in FindMatchesAtState (q)) {
                        foreach (var item in f.Items)
                            if (!done.Contains (item)) {
                                done.Add (item);
                                yield return item;
                            }
                    }
                } else {
                    yield break;
                }
            }

            if (DoFindContains) {
                var fc = FindBucketContains (key);
                return fc == null ? TOf (null) : fc.SelectMany (q => TOf (q)).Distinct ();
            } else {
                return TOf (FindBucketStartWith (key));
            }
        }

        public bool ContainsKey (string key) {
            return FindBucketStartWith (key) != null;
        }

        public bool ContainsItems (string key) {
            var q = FindBucketStartWith (key);
            if (q != null) {
                if (q.Items != null && q.Items.Count > 0)
                    return true;
                foreach (var f in FindMatchesAtState (q)) {
                    if (f.Items != null && f.Items.Count > 0)
                        return true;
                }
            }
            return false;
        }

        public bool ContainsItemsAt (string key) {
            var q = FindBucketStartWith (key);
            return q != null && q.Items != null && q.Items.Count > 0;
        }

        public int MaxLength {
            get { return max_length; }
        }

        public bool IsEmpty () {
            return Root.Next == null && Root.Sibling == null;
        }
    }

    /// <summary>
    /// link object in the TrieTree
    /// eg: keys: and, are
    ///     a -[Next]-> n         -[Next]-> d
    ///                 |[Sibling]
    ///                 r         -[Next]-> e
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrieBucket<T> {
        /// <summary>
        /// linkedlist of possible variants
        /// </summary>
        public TrieBucket<T> Sibling;

        /// <summary>
        /// go down in tree
        /// first bucket of next possible char in word
        /// </summary>
        public TrieBucket<T> Next;

        public char Value;

        public ICollection<T> Items;

        public override string ToString () {
            return string.Format ("TrieBucket '{0}'", Value);
        }
    }
}