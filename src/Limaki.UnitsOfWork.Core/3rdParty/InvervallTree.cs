/*
 Copyright (c) Vojtěch Vondra
    https://github.com/vvondra/Interval-Tree
 
 Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
 to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Limaki.Common.Collections {

    public static class Interval {
        public static Interval<T> Create<T> (T start, T end) where T : struct, IComparable<T> => new Interval<T> (start, end);
        public static Interval<T> Create<T> ((T start, T end) i) where T : struct, IComparable<T> => new Interval<T> (i.start, i.end);
        public static IntervalTree<T> Create<T> (IEnumerable<(T, T)> entries) where T : struct, IComparable<T> => new IntervalTree<T> (entries.Select (e => Interval.Create (e.Item1, e.Item2)));
        public static List<Interval<T>> Search<T> (this IntervalTree<T> it, (T start, T end) i) where T : struct, IComparable<T> => it.Search (Create (i));
        public static Interval<T> SearchFirstOverlapping<T> (this IntervalTree<T> it, (T start, T end) i) where T : struct, IComparable<T> => it.SearchFirstOverlapping (Create (i));
        public static void Add<T> (this IntervalTree<T> it, (T start, T end) i) where T : struct, IComparable<T> => it.Add (Create (i));
    }

    /// <summary>
    /// Node of interval Tree
    /// </summary>
    /// <typeparam name="T">type of interval bounds</typeparam>
    internal class IntervalNode<T> : IComparable<IntervalNode<T>> where T : struct, IComparable<T> {

        public IntervalNode<T> Left { get; set; }
        public IntervalNode<T> Right { get; set; }
        public IntervalNode<T> Parent { get; set; }

        /// <summary>
        /// Maximum "end" value of interval in node subtree
        /// </summary>
        public T MaxEnd { get; set; }

        /// <summary>
        /// The interval this node holds
        /// </summary>
        public Interval<T> Interval;

        private NodeColor color;
        /// <summary>
        /// Color of the node used for R-B implementation
        /// </summary>
        public NodeColor Color {
            get { return color; }
            set {
                this.color = value;
            }

        }

        public IntervalNode () {
            Parent = Left = Right = IntervalTree<T>.Sentinel;
            Color = NodeColor.BLACK;
        }

        public IntervalNode (Interval<T> interval) : this () {
            MaxEnd = interval.End;
            Interval = interval;
        }

        /// <summary>
        /// Indicates wheter the node has a parent
        /// </summary>
        public bool IsRoot {
            get { return Parent == IntervalTree<T>.Sentinel; }
        }

        /// <summary>
        /// Indicator whether the node has children
        /// </summary>
        public bool IsLeaf {
            get { return Right == IntervalTree<T>.Sentinel && Left == IntervalTree<T>.Sentinel; }
        }

        /// <summary>
        /// The direction of the parent, from the child point-of-view
        /// </summary>
        public NodeDirection ParentDirection {
            get {
                if (Parent == IntervalTree<T>.Sentinel) {
                    return NodeDirection.NONE;
                }

                return Parent.Left == this ? NodeDirection.RIGHT : NodeDirection.LEFT;
            }
        }

        public IntervalNode<T> GetSuccessor () {
            if (Right == IntervalTree<T>.Sentinel) {
                return IntervalTree<T>.Sentinel;
            }

            var node = Right;
            while (node.Left != IntervalTree<T>.Sentinel) {
                node = node.Left;
            }

            return node;
        }

        public int CompareTo (IntervalNode<T> other) {
            return Interval.CompareTo (other.Interval);
        }

        /// <summary>
        /// Refreshes the MaxEnd value after node manipulation
        /// 
        /// This is a local operation only
        /// </summary>
        public void RecalculateMaxEnd () {
            T max = Interval.End;

            if (Right != IntervalTree<T>.Sentinel) {
                if (Right.MaxEnd.CompareTo (max) > 0) {
                    max = Right.MaxEnd;
                }
            }

            if (Left != IntervalTree<T>.Sentinel) {
                if (Left.MaxEnd.CompareTo (max) > 0) {
                    max = Left.MaxEnd;
                }
            }

            MaxEnd = max;

            if (Parent != IntervalTree<T>.Sentinel) {
                Parent.RecalculateMaxEnd ();
            }
        }

        /// <summary>
        /// Return grandparent node
        /// </summary>
        /// <returns>grandparent node or IntervalTree<T>.Sentinel if none</returns>
        public IntervalNode<T> GrandParent {
            get {
                if (Parent != IntervalTree<T>.Sentinel) {
                    return Parent.Parent;
                }
                return IntervalTree<T>.Sentinel;
            }
        }

        /// <summary>
        /// Returns sibling of parent node
        /// </summary>
        /// <returns>sibling of parent node or IntervalTree<T>.Sentinel if none</returns>
        public IntervalNode<T> Uncle {
            get {
                var gparent = GrandParent;
                if (gparent == IntervalTree<T>.Sentinel) {
                    return IntervalTree<T>.Sentinel;
                }

                if (Parent == gparent.Left) {
                    return gparent.Right;
                }

                return gparent.Left;
            }
        }

        /// <summary>
        /// Returns sibling node
        /// </summary>
        /// <returns>sibling node or IntervalTree<T>.Sentinel if none</returns>
        public IntervalNode<T> Sibling {
            get {
                if (Parent != IntervalTree<T>.Sentinel) {
                    if (Parent.Right == this) {
                        return Parent.Left;
                    }

                    return Parent.Right;
                }

                return IntervalTree<T>.Sentinel;
            }
        }
    }

    internal enum NodeColor {
        RED,
        BLACK
    }

    internal enum NodeDirection {
        LEFT,
        RIGHT,
        NONE
    }

    /// <summary>
    /// Representation of bounded interval
    /// </summary>
    /// <typeparam name="T">type of interval bounds</typeparam>
    public struct Interval<T> : IComparable<Interval<T>> where T : struct, IComparable<T> {

        public T Start {
            get;
            set;
        }

        public T End {
            get;
            set;
        }

        public Interval (T start, T end)
            : this () {
            Start = start;
            End = end;

            if (Start.CompareTo (End) > 0) {
                throw new ArgumentException ("Start cannot be larger than End of interval");
            }
        }

        /// <summary>
        /// Tests if interval contains given interval
        /// </summary>
        public bool Contains (Interval<T> interval) {
            return Start.CompareTo (interval.Start) <= 0 && End.CompareTo (interval.End) >= 0;
        }

        /// <summary>
        /// Tests if interval contains given value
        /// </summary>
        public bool Contains (T val) {
            return Start.CompareTo (val) <= 0 && End.CompareTo (val) >= 0;
        }

        public bool Overlaps (Interval<T> interval) {
            return Start.CompareTo (interval.End) <= 0 && End.CompareTo (interval.Start) >= 0;
        }

        /// <summary>
        /// Porovná dva intervaly
        /// 
        /// Lineární uspořádání je def. 1) pořadím počátků 2) pořadím konců
        /// </summary>
        /// <param name="i">interval k porovnání</param>
        /// <returns></returns>
        public int CompareTo (Interval<T> i) {
            if (Start.CompareTo (i.Start) < 0) {
                return -1;
            } else if (Start.CompareTo (i.Start) > 0) {
                return 1;
            } else if (End.CompareTo (i.End) < 0) {
                return 1;
            } else if (End.CompareTo (i.End) > 0) {
                return -1;
            }

            // Identical interval
            return 0;
        }

        public static bool operator == (Interval<T> c1, Interval<T> c2) => c1.CompareTo (c2) == 0;

        public static bool operator != (Interval<T> c1, Interval<T> c2) => !c1.Equals (c2);

        public override string ToString () {
            return String.Format ("<{0}, {1}>", Start.ToString (), End.ToString ());
        }

        public override bool Equals (object obj) {
            if (!(obj is Interval<T> other)) {
                return false;
            }
            return this == other;
        }

        public override int GetHashCode () {
            var hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode (Start);
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode (End);
            return hashCode;
        }
    }

    public partial class IntervalTree<T> : IEnumerable<Interval<T>> where T : struct, IComparable<T> {

        internal static IntervalNode<T> Sentinel = new IntervalNode<T> (new Interval<T> ());

        IntervalNode<T> Root {
            get;
            set;
        }

        public IntervalTree () {
            Root = Sentinel;
            Root.Left = Sentinel;
            Root.Right = Sentinel;
            Root.Parent = Sentinel;
        }

        public IntervalTree (IEnumerable<Interval<T>> entries) : this () {
            foreach (var interval in entries) {
                Add (interval);
            }
        }

        #region Tree searching
        /// <summary>
        /// Search interval tree for a given point
        /// </summary>
        /// <param name="val">value to be searched for</param>
        /// <returns>list of intervals which contain the value</returns>
        public List<Interval<T>> Search (T val) {
            var result = new List<Interval<T>> ();
            SearchSubtree (Root, val, result);
            return result;
        }

        /// <summary>
        /// Search interval tree for intervals overlapping with given
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public List<Interval<T>> Search (Interval<T> i) {
            var result = new List<Interval<T>> ();
            SearchSubtree (Root, i, result);
            return result;
        }


        /// <summary>
        /// Searches for the first overlapping interval
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Interval<T> SearchFirstOverlapping (Interval<T> i) {
            var node = Root;

            while (node != Sentinel && !node.Interval.Overlaps (i)) {
                if (node.Left != Sentinel && node.Left.MaxEnd.CompareTo (i.Start) >= 0) {
                    node = node.Left;
                } else {
                    node = node.Right;
                }
            }

            if (node == Sentinel) {
                return default; //throw new KeyNotFoundException ("No overlapping interval found.");
            }

            return node.Interval;
        }

        private void SearchSubtree (IntervalNode<T> node, Interval<T> i, List<Interval<T>> result) {
            if (node == Sentinel) {
                return;
            }

            if (node.Left != Sentinel) {
                SearchSubtree (node.Left, i, result);
            }

            if (i.Overlaps (node.Interval)) {
                result.Add (node.Interval);
            }

            // Interval start is greater than largest endpoint in this subtree
            if (node.Right != Sentinel && i.Start.CompareTo (node.MaxEnd) <= 0) {
                SearchSubtree (node.Right, i, result);
            }
        }

        private IntervalNode<T> FindInterval (IntervalNode<T> tree, Interval<T> i) {

            while (tree != Sentinel) {

                if (tree.Interval.CompareTo (i) > 0) {
                    tree = tree.Left;
                    continue;
                }

                if (tree.Interval.CompareTo (i) < 0) {
                    tree = tree.Right;
                    continue;
                }

                if (tree.Interval.CompareTo (i) == 0) {
                    return tree;
                }
            }


            return Sentinel;

        }

        /// <summary>
        /// Recursively descends down the tree and adds valids results to the resultset
        /// </summary>
        /// <param name="node">subtree to be searched</param>
        /// <param name="val">value to be searched for</param>
        /// <param name="result">current resultset</param>
        private void SearchSubtree (IntervalNode<T> node, T val, List<Interval<T>> result) {
            if (node == Sentinel) {
                return;
            }

            // Value is higher than any interval in this subtree
            if (val.CompareTo (node.MaxEnd) > 0) {
                return;
            }

            if (node.Left != Sentinel) {
                SearchSubtree (node.Left, val, result);
            }

            if (node.Interval.Contains (val)) {
                result.Add (node.Interval);
            }

            if (val.CompareTo (node.Interval.Start) < 0) {
                return;
            }

            if (node.Right != Sentinel) {
                SearchSubtree (node.Right, val, result);
            }
        }

        #endregion search

        /// <summary>
        /// Insert new interval to interval tree
        /// </summary>
        /// <param name="interval">interval to add</param>
        public void Add (Interval<T> interval) {
            var node = new IntervalNode<T> (interval);
            if (Root == Sentinel) {
                node.Color = NodeColor.BLACK;
                Root = node;
            } else {
                InsertInterval (interval, Root);
            }
        }

        #region Tree insertion internals

        /// <summary>
        /// Recursively descends to the correct spot for interval insertion in the tree
        /// When a free spot is found for the node, it is attached and tree state is validated
        /// </summary>
        /// <param name="interval">interval to be added</param>
        /// <param name="currentNode">subtree accessed in recursion</param>
        private void InsertInterval (Interval<T> interval, IntervalNode<T> currentNode) {
            IntervalNode<T> addedNode = Sentinel;
            if (interval.CompareTo (currentNode.Interval) < 0) {
                if (currentNode.Left == Sentinel) {
                    addedNode = new IntervalNode<T> (interval);
                    addedNode.Color = NodeColor.RED;
                    currentNode.Left = addedNode;
                    addedNode.Parent = currentNode;
                } else {
                    InsertInterval (interval, currentNode.Left);
                    return;
                }
            } else if (interval.CompareTo (currentNode.Interval) > 0) {
                if (currentNode.Right == Sentinel) {
                    addedNode = new IntervalNode<T> (interval);
                    addedNode.Color = NodeColor.RED;
                    currentNode.Right = addedNode;
                    addedNode.Parent = currentNode;
                } else {
                    InsertInterval (interval, currentNode.Right);
                    return;
                }
            } else {
                return;
            }

            addedNode.Parent.RecalculateMaxEnd ();

            RenewConstraintsAfterInsert (addedNode);

            Root.Color = NodeColor.BLACK;
        }

        /// <summary>
        /// Validates and applies RB-tree constaints to node 
        /// </summary>
        /// <param name="node">node to be validated and fixed</param>
        private void RenewConstraintsAfterInsert (IntervalNode<T> node) {
            if (node.Parent == Sentinel) {
                return;
            }

            if (node.Parent.Color == NodeColor.BLACK) {
                return;
            }

            var uncle = node.Uncle;

            if (uncle != Sentinel && uncle.Color == NodeColor.RED) {
                node.Parent.Color = uncle.Color = NodeColor.BLACK;

                var gparent = node.GrandParent;
                if (gparent != Sentinel && !gparent.IsRoot) {
                    gparent.Color = NodeColor.RED;
                    RenewConstraintsAfterInsert (gparent);
                }
            } else {
                if (node.ParentDirection == NodeDirection.LEFT && node.Parent.ParentDirection == NodeDirection.RIGHT) {
                    RotateLeft (node.Parent);
                    node = node.Left;
                } else if (node.ParentDirection == NodeDirection.RIGHT && node.Parent.ParentDirection == NodeDirection.LEFT) {
                    RotateRight (node.Parent);
                    node = node.Right;
                }

                node.Parent.Color = NodeColor.BLACK;

                if (node.GrandParent == Sentinel) {
                    return;
                }
                node.GrandParent.Color = NodeColor.RED;

                if (node.ParentDirection == NodeDirection.RIGHT) {
                    RotateRight (node.GrandParent);
                } else {
                    RotateLeft (node.GrandParent);
                }
            }

        }

        #endregion Tree insertion internals

        /// <summary>
        /// Removes interval from tree (if present in tree)
        /// </summary>
        /// <param name="?"></param>
        public void Remove (Interval<T> interval) {
            RemoveNode (FindInterval (Root, interval));
        }

        private void RemoveNode (IntervalNode<T> node) {
            if (node == Sentinel) {
                return;
            }

            IntervalNode<T> temp = node;
            if (node.Right != Sentinel && node.Left != Sentinel) {
                // Trick when deleting node with both children, switch it with closest in order node
                // swap values and delete the bottom node converting it to other cases

                temp = node.GetSuccessor ();
                node.Interval = temp.Interval;

                node.RecalculateMaxEnd ();
                while (node.Parent != Sentinel) {
                    node = node.Parent;
                    node.RecalculateMaxEnd ();
                }
            }
            node = temp;
            temp = node.Left != Sentinel ? node.Left : node.Right;

            // we will replace node with temp and delete node
            temp.Parent = node.Parent;
            if (node.IsRoot) {
                Root = temp; // Set new root
            } else {

                // Reattach node to parent
                if (node.ParentDirection == NodeDirection.RIGHT) {
                    node.Parent.Left = temp;
                } else {
                    node.Parent.Right = temp;
                }

                IntervalNode<T> maxAux = node.Parent;
                maxAux.RecalculateMaxEnd ();
                while (maxAux.Parent != Sentinel) {
                    maxAux = maxAux.Parent;
                    maxAux.RecalculateMaxEnd ();
                }
            }

            if (node.Color == NodeColor.BLACK) {
                RenewConstraintsAfterDelete (temp);
            }

        }

        /// <summary>
        /// Ensures constraints still apply after node deletion
        /// 
        ///  - made with the help of algorithm from Cormen et Al. Introduction to Algorithms 2nd ed.
        /// </summary>
        /// <param name="node"></param>
        private void RenewConstraintsAfterDelete (IntervalNode<T> node) {
            // Need to bubble up and fix
            while (node != Root && node.Color == NodeColor.BLACK) {
                if (node.ParentDirection == NodeDirection.RIGHT) {
                    IntervalNode<T> aux = node.Parent.Right;
                    if (aux.Color == NodeColor.RED) {
                        aux.Color = NodeColor.BLACK;
                        node.Parent.Color = NodeColor.RED;
                        RotateLeft (node.Parent);
                        aux = node.Parent.Right;
                    }

                    if (aux.Left.Color == NodeColor.BLACK && aux.Right.Color == NodeColor.BLACK) {
                        aux.Color = NodeColor.RED;
                        node = node.Parent;
                    } else {
                        if (aux.Right.Color == NodeColor.BLACK) {
                            aux.Left.Color = NodeColor.BLACK;
                            aux.Color = NodeColor.RED;
                            RotateRight (aux);
                            aux = node.Parent.Right;
                        }

                        aux.Color = node.Parent.Color;
                        node.Parent.Color = NodeColor.BLACK;
                        aux.Right.Color = NodeColor.BLACK;
                        RotateLeft (node.Parent);
                        node = Root;
                    }
                } else {
                    IntervalNode<T> aux = node.Parent.Left;
                    if (aux.Color == NodeColor.RED) {
                        aux.Color = NodeColor.BLACK;
                        node.Parent.Color = NodeColor.RED;
                        RotateRight (node.Parent);
                        aux = node.Parent.Left;
                    }

                    if (aux.Left.Color == NodeColor.BLACK && aux.Right.Color == NodeColor.BLACK) {
                        aux.Color = NodeColor.RED;
                        node = node.Parent;
                    } else {
                        if (aux.Left.Color == NodeColor.BLACK) {
                            aux.Right.Color = NodeColor.BLACK;
                            aux.Color = NodeColor.RED;
                            RotateLeft (aux);
                            aux = node.Parent.Left;
                        }

                        aux.Color = node.Parent.Color;
                        node.Parent.Color = NodeColor.BLACK;
                        aux.Left.Color = NodeColor.BLACK;
                        RotateRight (node.Parent);
                        node = Root;
                    }
                }
            }

            node.Color = NodeColor.BLACK;
        }

        /// <summary>
        /// General right rotation
        /// </summary>
        /// <param name="node">Top of rotated subtree</param>
        private void RotateRight (IntervalNode<T> node) {
            var pivot = node.Left;
            NodeDirection dir = node.ParentDirection;
            var parent = node.Parent;
            var tempTree = pivot.Right;
            pivot.Right = node;
            node.Parent = pivot;
            node.Left = tempTree;
            if (tempTree != Sentinel) {
                tempTree.Parent = node;
            }

            if (dir == NodeDirection.LEFT) {
                parent.Right = pivot;
            } else if (dir == NodeDirection.RIGHT) {
                parent.Left = pivot;
            } else {
                Root = pivot;
            }

            pivot.Parent = parent;

            pivot.RecalculateMaxEnd ();
            node.RecalculateMaxEnd ();

        }

        /// <summary>
        /// General left rotation
        /// </summary>
        /// <param name="node">top of rotated subtree</param>
        private void RotateLeft (IntervalNode<T> node) {
            var pivot = node.Right;
            NodeDirection dir = node.ParentDirection;
            var parent = node.Parent;
            var tempTree = pivot.Left;
            pivot.Left = node;
            node.Parent = pivot;
            node.Right = tempTree;
            if (tempTree != Sentinel) {
                tempTree.Parent = node;
            }

            if (dir == NodeDirection.LEFT) {
                parent.Right = pivot;
            } else if (dir == NodeDirection.RIGHT) {
                parent.Left = pivot;
            } else {
                Root = pivot;
            }

            pivot.Parent = parent;

            pivot.RecalculateMaxEnd ();
            node.RecalculateMaxEnd ();
        }

        #region Enumerators
        private IEnumerable<Interval<T>> InOrderWalk (IntervalNode<T> node) {
            if (node.Left != Sentinel) {
                foreach (Interval<T> val in InOrderWalk (node.Left)) {
                    yield return val;
                }
            }

            if (node != Sentinel) {
                yield return node.Interval;
            }

            if (node.Right != Sentinel) {
                foreach (Interval<T> val in InOrderWalk (node.Right)) {
                    yield return val;
                }
            }
        }

        public IEnumerator<Interval<T>> GetEnumerator () {
            foreach (Interval<T> val in InOrderWalk (Root)) {
                yield return val;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }

        #endregion
    }
}

