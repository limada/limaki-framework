/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Runtime.Serialization;

namespace Limaki.Common.UnitsOfWork {
    [DataContract]
    public class State : ICloneable {
        public State() {}

        bool _creating = true;
        /// <summary>
        /// just created 
        /// </summary>
        [DataMember]
        public bool Creating {
            get { return _creating; }
            set {
                _creating = value;
            }
        }
        
        
        bool _clean = false;
        /// <summary>
        /// is safed in dataStore and not changed since then
        /// sets creating/dirty/hollow false if true
        /// </summary>
        [DataMember]
        public bool Clean {
            get { return _clean; }
            set {
                if (value && Dirty)
                    _dirty = false;
                if (value && Hollow)
                    _hollow = false;
                if (value && Creating)
                    _creating = false;
                _clean = value;
            }
        }

        bool _hollow = false;

        /// <summary>
        /// never saved in a dataStore until now
        /// </summary>
        [DataMember]
        public bool Hollow {
            get { return _hollow; }
            set {
                if (!value && _hollow) {
                    _dirty = false;
                    _clean = false;
                }
                if (value && Creating)
                    _creating = false;
                _hollow = value;
            }
        }

        bool _dirty = false;
        /// <summary>
        /// changed; sets clean = false
        /// hollow remains unchanged!
        /// </summary>
        [DataMember]
        public bool Dirty {
            get { return _dirty; }
            set {
                if (!Creating) {
                    if (value && Clean)
                        _clean = false;
                    _dirty = value;
                }
            }
        }

        Action _setDirty = null;
        public Action SetDirty {
            get {
                if (_setDirty == null) {
                    _setDirty = () => { this.Dirty = true; };
                }
                return _setDirty;

            }
            set { _setDirty = value; }
        }
        public void Setter<T>(ref T target, T value) {
            if ((!object.Equals(value,target))) {
                target = value;
                SetDirty();
            }
        }

        #region ICloneable Member

        public object Clone() {
            State result = new State();
            result.Clean = this.Clean;
            result.Creating = this.Creating;
            result.Dirty = this.Dirty;
            result.Hollow = this.Hollow;
            return result;
        }

        #endregion
    }
}