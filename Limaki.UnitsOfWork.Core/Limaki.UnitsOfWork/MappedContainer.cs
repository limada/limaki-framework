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

using System.Runtime.Serialization;
using System;
using Limaki.Common.Collections;

namespace Limaki.Common.UnitsOfWork {

    [DataContract]
    public abstract class MappedContainer : ListContainer {
        
        protected abstract IFactory CreateFactory();
        bool _mapOwner = true;
        protected bool MapOwner { get { return _mapOwner; } }
        IdentityMap _map = null;
        public IdentityMap Map {
            get { return _map ?? (_map = new IdentityMap { ItemFactory = CreateFactory() }); }
            set {
                _map = value;
                _mapOwner = false;
            }
        }

        public virtual void Set<T>() {
            var list = ListProperty<T>();
            if (list != null) {
                list.SetValue(this, this.Map.Stored<T>(), null);
            } else {
                throw new ArgumentException(this.GetType() + " has no " + typeof(T).Name + " IEnumerable-Property");
            }
        }

        public abstract void ExpandItems();

        public override void Dispose(bool disposing) {
            if (_mapOwner && _map != null) {
                _map.Dispose(disposing);
            }
            _map = null;
            base.Dispose(disposing);
        }

        ~MappedContainer() {
            Dispose(false);
        }
    }
}