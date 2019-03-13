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

using System;
using System.Collections.Generic;

namespace Limaki.Common {

    public interface IFactory {

        T Create<T>();
        T Create<T>(params object[] args);
        object Create(Type type);

        void Add<T>(Func<T> creator);
        void Add<T>(Func<object[], T> creator);
        void Add<T1, T2>() where T2 : T1;
        void Add(Type source, Type target);

        Delegate Func<T>();

        Type Clazz<T>();
        IEnumerable<Type> KnownClasses { get; }
        
        bool Contains<T>();
        bool Contains(Type type);
        
        IFactory Clone();

    }
}