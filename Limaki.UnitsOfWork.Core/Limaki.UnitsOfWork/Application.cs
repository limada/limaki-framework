/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */


using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Limaki.Common.IOC;

namespace Limaki.UnitsOfWork {

    public interface IApplicationEngine {

        void DispatchPendingEvents ();
        void Invoke (Action a);
        Task InvokeAsync (Action a);
        Task<T> InvokeAsync<T> (Func<T> a);
        void NotifyException (Exception ex);
        void QueueExitAction (Action action);

    }

    public static class Application {

        static IApplicationEngine Engine { get; set; }

        public static void Initialize (IApplicationEngine engine) {
            Engine = engine;
            if (false) {
                // not working:
                if (mutex == null) {
                    mutex = new System.Threading.Mutex (false, AppId.ToString ());
                }
                AppDomain.CurrentDomain.DomainUnload += (sender, e) => {
                    if (mutex != null) {
                        mutex.Close ();
                        mutex = null;
                    }
                };
            }
        }

        public static void DispatchPendingEvents () => Engine.DispatchPendingEvents ();

        /// <summary>
        /// Invokes an action in the GUI thread
        /// </summary>
        public static void Invoke (Action action) => Engine.Invoke (action);

        /// <summary>
        /// Invokes an action in the GUI thread.
        /// </summary>
        public static Task InvokeAsync (Action action) => Engine.InvokeAsync (action);

        public static Task<T> InvokeAsync<T> (Func<T> func) => Engine.InvokeAsync (func);

        /// <summary>
        /// Adds the action to the exit action queue.
        /// </summary>
        /// <param name="a">The action to invoke after processing user code.</param>
        public static void QueueExitAction (Action action) => Engine.QueueExitAction (action);

        public static Guid AppId { get; set; } = new Guid ("755849ce-da87-40d3-9fdd-edf489fbad1c");
        static Mutex mutex { get; set; }

        /// <summary>
        /// checks if another instance is running
        /// NOT WORKING
        /// </summary>
        /// <returns><c>true</c>, if another instance is running, <c>false</c> otherwise.</returns>
        public static bool RunningInstance () {
            return false;
            return !mutex.WaitOne (0, false);
        }


    }
}