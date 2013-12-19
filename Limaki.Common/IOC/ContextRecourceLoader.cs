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



using System.Reflection;
using System;
using System.Linq;
using System.Diagnostics;

namespace Limaki.Common.IOC {
    /// <summary>
    /// instruments an IApplicationContext
    /// used to load the resources (common objects and factories) in an application
    /// this class has to be implemented for each type of application
    /// </summary>
    public abstract class ContextRecourceLoader : IContextRecourceLoader {
        /// <summary>
        /// instruments the context
        /// </summary>
        /// <param name="context"></param>
        public abstract void ApplyResources(IApplicationContext context);

        public virtual IApplicationContext CreateContext() {
            IApplicationContext result = new ApplicationContext();
            return result;
        }

        public virtual void LoadPluginAssemblies(){}

        public virtual void ApplyPluginResources(IApplicationContext context, Func<Assembly, bool> predicate) {
            LoadPluginAssemblies();
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies().Where(predicate)) {
                Trace.WriteLine(string.Format("ApplyPluginResources {0}", ass.FullName));
                foreach (var type in ass.GetTypes()) {//.Where(t => Reflector.Implements(t, typeof(IPluginContextRecourceLoader)))) {
                    if (Reflector.Implements(type, typeof(IPluginContextRecourceLoader))) {
                        try {
                            var loader = Activator.CreateInstance(type, null) as IPluginContextRecourceLoader;
                            Trace.WriteLine(string.Format("ApplyPluginResources {0}", type.Name));
                            loader.ApplyResources(context);
                        } catch (Exception ex) {
                            Trace.TraceError("Error loading plugin from type {0}", type.FullName);
                        }
                    }
                }
            }
        }
        
    }
}