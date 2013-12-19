/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2008-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.ServiceModel;
using System.Timers;
using System.Diagnostics;

namespace Limaki.Common.Services {

    public interface IWCFServer {

        IServiceSettings Settings { get; set; }
        bool KeepAlive { get; set; }

        void OpenServer();
        void CloseServer();
        void ServerTest();
    }

    public class WCFServer {
        protected static object sync = new object();
    }

    /// <summary>
    /// this is a handy class to implement a standalone server
    /// </summary>
    /// <typeparam name="I"></typeparam>
    /// <typeparam name="S"></typeparam>
    public class WCFServer<I, S> : WCFServer, IWCFServer
        where S : I {

        public virtual IServiceSettings Settings { get; set; }

        public ServiceHost<I, S> ServiceHost { get; set; }
        AsyncCallback serverThread = null;


        public virtual Func<ServiceHost<I, S>> CreateHost { get; set; }

        public WCFServer() {
            this.CreateHost = () => new ServiceHost<I, S> { Settings = this.Settings };
        }

        public virtual void OpenServer() {
            if (ServiceHost == null) {
                serverThread = new AsyncCallback(
                    (a) => {
                        ServiceHost = CreateHost();
                        if (this.ConfigureNetTcpBinding != null)
                            ServiceHost.ConfigureNetTcpBinding += (s, e) => this.ConfigureNetTcpBinding(s, e);
                        if (this.ConfigureWSHttpBinding != null)
                            ServiceHost.ConfigureWSHttpBinding += (s, e) => this.ConfigureWSHttpBinding(s, e);
                        if (this.ConfigureBasicHttpBinding != null)
                            ServiceHost.ConfigureBasicHttpBinding += (s, e) => this.ConfigureBasicHttpBinding(s, e);
                        ServiceHost.StartService();
                        System.Console.WriteLine("WCFServer started. " +
                                                 "\n\tBinding:\t" + ServiceHost.BindingType +
                                                 "\n\tAddress:\t" + ServiceHost.BaseAddress.ToString() +
                                                 "\n\tContract:\t" + ServiceHost.ServiceType);
                    });

                serverThread.Invoke(null);
            }

            if (KeepAlive) {
                timer = new Timer(new TimeSpan(0, 10, 0).TotalMilliseconds);
                timer.Elapsed += (s, e) => {
                    ServerTest();
                };

                timer.Start();
                GC.KeepAlive(timer);

            }
        }

        public event EventHandler<EventArgs<NetTcpBinding>> ConfigureNetTcpBinding;
        public event EventHandler<EventArgs<WSHttpBinding>> ConfigureWSHttpBinding;
        public event EventHandler<EventArgs<BasicHttpBinding>> ConfigureBasicHttpBinding;

        public virtual void CloseServer() {
            if (this.timer != null) {
                this.timer.Stop();
                this.timer.Close();
            }

            if (ServiceHost != null) {
                ServiceHost.StopService();
                Trace.WriteLine("server stopped");
            }
            if (serverThread != null) {
                serverThread = null;
            }
            ServiceHost = null;
        }


        public virtual void ServerTest() {
           
            var client = new ClientContext<I> { Settings = this.Settings };
            client.Open();

            var service = client.Service as IServiceBase;
            if (service != null) {
                service.Ping();
                var error = service.Test();
                client.Close();

                if (!string.IsNullOrEmpty(error)) {
                    throw new Exception(error);
                }
            }
        }
        
        public virtual ClientContext<I> Client() {
            var client = new ClientContext<I> { Settings = this.Settings };
            client.Open();
            return client;
        }

        public bool KeepAlive { get; set; }
        public Timer timer { get; set; }
    }
}