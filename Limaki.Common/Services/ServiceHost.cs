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
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Linq;

namespace Limaki.Common.Services {
    public class ServiceHost<TInterface, TImpl> : HostBase<TInterface> {

        protected ServiceHost _host = null;
        protected Binding _binding = null;

        public Binding Binding {
            get { return _binding; }
        }

        public virtual void StartService() {

            _binding = GetDefaultBinding();
            bool singleInstance = false;
            if (singleInstance) {
                // look in TImpl if ServiceBehavior InstanceContextMode.Single
                var service = Activator.CreateInstance<TImpl>();
                _host = new ServiceHost(service, BaseAddress);
            } else {
                _host = new ServiceHost(typeof(TImpl), BaseAddress);
            }
            var attr = typeof(TImpl).GetAttribute<ServiceBehaviorAttribute>();
            if (attr != null && !string.IsNullOrEmpty(attr.TransactionTimeout))
                SetTimeout(_binding, TimeSpan.Parse(attr.TransactionTimeout));


            var endPoint = _host.AddServiceEndpoint(
                typeof(TInterface),
                _binding,
                BaseAddress);


            if (this.BindingType == BindingFlag.BasicHttpBinding) {
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                _host.Description.Behaviors.Add(smb);
            }

            ConfigureHost(_host);

            try {
                _host.Open();
                Debug.WriteLine("The service is ready." +
                                "\tBinding:\t" + _binding.GetType().Name +
                                "\tAddress:\t" + BaseAddress.ToString() +
                                "\tContract:\t" + typeof(TInterface)

                    );

            } catch (TimeoutException timeProblem) {
                Debug.WriteLine(timeProblem.Message);

            } catch (CommunicationException commProblem) {
                Debug.WriteLine(commProblem.Message);
            } catch (ThreadAbortException ex) {
                Debug.WriteLine(ex.Message);
                _host.Close();
            } catch (Exception eall) {
                Debug.WriteLine(eall.Message);
                _host.Close();
            }
        }

        public void StopService() {
            if (this._host != null && this._host.State == CommunicationState.Opened) {
                this._host.Close();
            }
        }
    }
}