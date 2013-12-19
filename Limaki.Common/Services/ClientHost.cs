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
using System.ServiceModel.Channels;

namespace Limaki.Common.Services {
    public class ClientHost<TInterface> : HostBase<TInterface> {
        protected ChannelFactory<TInterface> factory = null;
        public void Open() {

            var binding = this.GetDefaultBinding();

            factory = new ChannelFactory<TInterface>(binding, new EndpointAddress(BaseAddress));
            ConfigureFactory(factory);
            factory.Open();

            this.Service = factory.CreateChannel();

            ConfigureTrace();

        }

        public virtual void Close() {
            if (factory != null) {
                try {
                    if (factory.State == CommunicationState.Opened) {
                        factory.Close(new TimeSpan(0,0,10));
                    }
                } catch { }
            }
            factory = null;
        }

        public virtual void Abort() {
            if (factory != null) {
                try {
                    if (factory.State == CommunicationState.Opened) {
                        factory.Abort();
                    }
                } catch { }
            }
            factory = null;
        }
    }

    /// <summary>
    /// This is an abstract ClientHost class for a concrete clientservice implementation
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TClientImpl"></typeparam>
    public abstract class ClientHost<TInterface, TClientImpl> : HostBase<TInterface>
        where TClientImpl : class, ICommunicationObject, TInterface {

        protected TClientImpl ClientService = null;

        public abstract TClientImpl CreateClientService(Binding binding, EndpointAddress remoteAddress);

        public void Open() {

            var binding = this.GetDefaultBinding();

            ClientService = CreateClientService(binding, new EndpointAddress(BaseAddress));
            this.Service = ClientService;
            ClientService.Open();


        }


        public virtual void Close() {
            if (ClientService != null) {
                ClientService.Close();
            }

        }
    }
}