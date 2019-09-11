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
using Limaki.Common;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork {

    [DataContract]
    public class MappedContainer : ListContainer, IMappedContainer {

        protected MappedContainer (IFactory factory) {
            Factory = factory;
        }

        protected IFactory Factory { get; set; }

		protected bool MapOwner { get; set; } = true;

        IdentityMap _map = null;
        public IdentityMap Map {
            get { return _map ?? (_map = new IdentityMap { ItemFactory = Factory }); }
            set {
                _map = value;
                MapOwner = false;
            }
        }

        public virtual void Set<T>() {
            var list = ListProperty<T>();
            if (list != null) {
                list.SetValue(this, Map.Stored<T>(), null);
            } else {
                throw new ArgumentException(this.GetType() + " has no " + typeof(T).Name + " IEnumerable-Property");
            }
        }

        public override void Dispose(bool disposing) {
            if (MapOwner) {
                _map?.Dispose(disposing);
            }
            _map = null;
            base.Dispose(disposing);
        }

        ~MappedContainer() {
            Dispose(false);
        }
    }
}