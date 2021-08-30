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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace Limaki.UnitsOfWork {

    [DataContract]
    public abstract class ChangeSet {
        public abstract void Clear ();
        public abstract bool HasData { get; }

	}

    [DataContract]
    public class ChangeSet<T>:ChangeSet {

        static ChangeSet() {
            typeof(ChangeSet<T>).GetCustomAttributes(false).OfType<DataContractAttribute>().First().Name = nameof(ChangeSet)+typeof(T).Name;
        }
        protected ICollection<T> _created = null;
        [DataMember]
        public virtual ICollection<T> Created {
            get {
                if (_created == null) {
                    _created = new HashSet<T>();
                }
                return _created;
            }
            set { _created = value; }
        }

        protected ICollection<T> _updated = null;
        [DataMember]
        public virtual ICollection<T> Updated {
            get {
                if (_updated == null) {
                    _updated = new HashSet<T>();
                }
                return _updated;
            }
            set { _updated = value; }
        }

        protected ICollection<T> _removed = null;
        [DataMember]
        public virtual ICollection<T> Removed {
            get {
                if (_removed == null) {
                    _removed = new HashSet<T>();
                }
                return _removed;
            }
            set { _removed = value; }
        }

        public override bool HasData {
            get {
                return (_created != null && _created.Count != 0) ||
                       (_removed != null && _removed.Count != 0) ||
                       (_updated != null && _updated.Count != 0);
            }
        }

        public override void Clear() {
            
            if (_created != null && _created.Count != 0) {
                if (!_created.IsReadOnly)
                    _created.Clear();
                _created = null;
            }
            if (_removed != null && _removed.Count != 0) {
                if (!_removed.IsReadOnly)
                    _removed.Clear();
                _removed = null;
            }
            if (_updated != null && _updated.Count != 0) {
                if (!_updated.IsReadOnly)
                    _updated.Clear();
                _updated = null;
            }
        }
    }
}