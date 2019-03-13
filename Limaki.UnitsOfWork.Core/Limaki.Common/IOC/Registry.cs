/*
 * Limaki 
 * Version 0.081
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2008 Lytico
 *
 * http://www.limada.org
 * 
 */

using Limaki.Common.IOC;

namespace Limaki.Common {
    /// <summary>
    /// The Registry gathers application wide used objects
    /// IOC-Container
    /// </summary>
    public class Registry:IApplicationContext {
        static IApplicationContext _concreteContext = null;
        public static IApplicationContext ConcreteContext {
            get {
                if (_concreteContext == null)
                    _concreteContext = new ApplicationContext();
                return _concreteContext;
            }
            set { _concreteContext = value; }
        }

        public static IPool Pool {
            get { return ConcreteContext.Pool; }
        }

        public static IFactory Factory {
            get { return ConcreteContext.Factory; }
        }

        /// <summary>
        /// looks for an ApplicationContextProcessor in the Pool
        /// and calls ApplicationContextProcessor.ApplyProperties(target)
        /// </summary>
        /// <typeparam name="TProcessor"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="target"></param>
        public static void ApplyProperties<T>(T target){
            ContextProcessor<T> processor = 
                Pool.TryGetCreate < ContextProcessor<T>> ();
            processor.ApplyProperties (ConcreteContext, target);
        }

        /// <summary>
        /// looks for an ApplicationContextProcessor in the Pool
        /// and calls ApplicationContextProcessor.ApplyProperties(target)
        /// </summary>
        /// <typeparam name="TProcessor"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="target"></param>
        public static void ApplyProperties<TProcessor, TTarget>(TTarget target)
            where TProcessor : ContextProcessor<TTarget> {
            if (target == null || ConcreteContext==null) return;
            ContextProcessor<TTarget> processor = Pool.TryGetCreate<TProcessor>();
            processor.ApplyProperties(ConcreteContext, target);

        }

        #region IApplicationContext Member

        IPool IApplicationContext.Pool {
            get { return ConcreteContext.Pool; }
        }

        IFactory IApplicationContext.Factory {
            get { return ConcreteContext.Factory; }
        }

        #endregion
    }
}