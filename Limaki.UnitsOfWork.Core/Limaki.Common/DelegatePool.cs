/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2011 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Limaki.Common {

    public class DelegatePool {

        private IDictionary<Type, Delegate> _clazzes = null;
        protected IDictionary<Type, Delegate> Clazzes {
            get {
                if (_clazzes == null) {
                    _clazzes = new Dictionary<Type, Delegate>();
                    InstrumentClazzes();
                }
                return _clazzes;
            }
        }

        public bool Contains<T>() {
            return Clazzes.ContainsKey(typeof(T));
        }

        public Delegate Get<T>() {
            var type = typeof(T);
            Delegate result = null;
            if (Clazzes.TryGetValue(type, out result)) {
                return result;
            }

            return null;
        }

        public TDelegate Get<T, TDelegate>(){
            var result = Get<T> ();
            if (result != null && result.GetType().Equals(typeof(TDelegate)))
                return (TDelegate)(object)Get<T>();
            return default( TDelegate );
        }

        public void Add<T, TDelegate>(Expression<TDelegate> expression ) {
            Clazzes[typeof(T)] = expression.Compile() as Delegate;
        }


        protected virtual void InstrumentClazzes() { }


    }
}