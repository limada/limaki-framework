/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2010 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;

namespace Limaki.Common {
    /// <summary>
    /// wrapps a collection
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class MappingCollection<S, T> : ICollection<S>, IMappingCollection {

        public MappingCollection(ICollection<T> sources) {
            this.Sources = sources;
        }

        public Func<S, T> SourceOf { get; set; }
        public Func<T, S> MapperOf { get; set; }

        public virtual ICollection<T> Sources { get; protected set; }

        public virtual void Add(S item) {
            Sources.Add(SourceOf(item));
        }

        public virtual void Clear() {
            Sources.Clear();
        }

        public virtual bool Contains(S item) {
            return Sources.Contains(SourceOf(item));
        }

        public virtual void CopyTo(S[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public virtual int Count {
            get { return Sources.Count; }
        }

        public virtual bool IsReadOnly {
            get { return Sources.IsReadOnly; }
        }

        public virtual bool Remove(S item) {
            return Sources.Remove(SourceOf(item));
        }

        public virtual IEnumerator<S> GetEnumerator() {
            return Sources.Select(s => MapperOf(s)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }


        System.Collections.IEnumerable IMappingCollection.Sources {
            get { return this.Sources; }
        }
    }

    public interface IMappingCollection {
        System.Collections.IEnumerable Sources { get; }
    }

    public class MappingList<S, T> : MappingCollection<S, T>, IList<S> {

        public MappingList(ICollection<T> list) : this(list as IList<T>) { }

        public MappingList(IList<T> list): base(list) {
            if (list == null)
                throw new ArgumentException();
        }

        public virtual IList<T> List { get { return this.Sources as IList<T>; } }

        public virtual int IndexOf(S item) {
            return List.IndexOf(SourceOf(item));
        }

        public virtual void Insert(int index, S item) {
            List.Insert(index, SourceOf(item));
        }

        public virtual void RemoveAt(int index) {
            List.RemoveAt(index);
        }

        public virtual S this[int index] {
            get {
                return MapperOf(List[index]);
            }
            set {
                List[index] = SourceOf(value);
            }
        }
    }

    
}