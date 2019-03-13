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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;

namespace Limaki.Common.Services {

    public interface IHost {
        Uri BaseAddress { get; }
        Type ServiceType { get; }
        BindingFlag BindingType { get; }
    }

    public class HostBase<T>:IHost {
        public HostBase() {
            MaxReceivedMessageSize = 1024 * 1024 * 1024;
            MaxItemsInObjectGraph = int.MaxValue;
            Tracing = false;
            Timeout = new TimeSpan(0, 1, 0);
        }

        
        public virtual IServiceSettings Settings { get;set;}
        public TimeSpan Timeout { get; set; }

        public string ServicePath {
            get { return "/" + Settings.Prefix + "/" + typeof(T).Name + "/"; }
        }

        Uri _baseAddress = null;
        public Uri BaseAddress {
            get {
                if (_baseAddress == null) {
                    var uri = GetBindingAdressPrefix(Settings.Binding) +
                              Settings.IP + ":" + Settings.Port + ServicePath;
                    _baseAddress = new Uri(uri);
                }
                return _baseAddress;
            }
        }

        public Type ServiceType { get { return typeof(T); } }

        public T Service { get; set; }

        public virtual BindingFlag BindingType { get; set; }
        public virtual string GetBindingAdressPrefix(BindingFlag bindingtype) {
            if (Settings.Binding == BindingFlag.NetTcp) {
                return "net.tcp://";
            } else if (Settings.Binding == BindingFlag.BasicHttpBinding) {
                return "http://";
            }
            return "http://";
        }

        public virtual Binding GetDefaultBinding() {
            Binding result = null;
            BindingType = Settings.Binding;

            _baseAddress = null;

            if (Settings.Binding == BindingFlag.NetTcp) {
                var binding = new NetTcpBinding();
                binding.Security.Mode = SecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                ConfigureBinding(binding);
                result = binding;

            } else if (Settings.Binding == BindingFlag.BasicHttpBinding) {
                var binding = new BasicHttpBinding();
                ConfigureBinding(binding);
                //binding.Security.Mode = BasicHttpSecurityMode.Message;
                //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                result = binding;
            }

            SetTimeout(result,  Settings.Timeout == default(TimeSpan) ? this.Timeout : Settings.Timeout);

            ConfigureTrace();
            return result;
        }

        public void SetTimeout(Binding result, TimeSpan timeout) {
            result.ReceiveTimeout = timeout;
            result.OpenTimeout = timeout;
            result.SendTimeout = timeout;
            result.CloseTimeout = timeout;
        }

        public bool Tracing { get; set; }
        public string TraceFile { get; set; }
        static TraceSource ts = null;

        protected virtual void ConfigureTrace () {
            if (Tracing && ts == null) {
                
                if(TraceFile == null)
                    TraceFile = Assembly.GetEntryAssembly().ManifestModule.Name;

                //var listener = new System.Diagnostics.ConsoleTraceListener();
                //listener.Name = "console";
                 
                //listener.TraceOutputOptions |= TraceOptions.Callstack;
                //listener.TraceOutputOptions = TraceOptions.DateTime;
                //var filter = new SourceFilter("System.Runtime.Serialization");
                //listener.Filter = filter;

                //Trace.Listeners.Add(listener);
                //Trace.AutoFlush = true;
                //Trace.TraceInformation("Service Trace started");

                //ts = new TraceSource("System.Runtime.Serialization");
                //SourceSwitch sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
                //ts.Switch = sourceSwitch;
                //ts.Listeners.Add(listener);
                //ts.TraceInformation("Service Trace started");

                BindingFlags privateMember = BindingFlags.NonPublic | BindingFlags.Instance;
                BindingFlags privateStaticMember = privateMember | BindingFlags.Static;

                Type type = //typeof (System.ServiceModel.DiagnosticUtility);
                    Type.GetType("System.ServiceModel.DiagnosticUtility, System.ServiceModel, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    //Type.GetType("System.ServiceModel.DiagnosticUtility");
                MethodInfo[] mi = type.GetMethods(privateStaticMember);


                     // invoke InitializeTracing  
                mi.FirstOrDefault(e => e.Name == "InitDiagnosticTraceImpl").Invoke(null, new object[] { 0, "System.ServiceModel" });
                object diagnosticTrace = type.GetField("diagnosticTrace", privateStaticMember).GetValue(null);
           
                // this does not work if there are no listeners
                //object diagnosticTrace = mi.FirstOrDefault(e => e.Name == "InitializeTracing").Invoke(null, null);
                
                if (diagnosticTrace != null) {
                    // get TraceSource  
                    Type type2 = Type.GetType("System.ServiceModel.Diagnostics.DiagnosticTrace, SMDiagnostics, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    PropertyInfo pi = type2.GetProperty("TraceSource", privateMember);
                    var traceSource = pi.GetValue(diagnosticTrace, null) as TraceSource;

                    ts = traceSource;
                    // clear all listeners in the trace source  
                    traceSource.Listeners.Clear();

                    // add listener to trace source  
                    XmlWriterTraceListener listener = new XmlWriterTraceListener(TraceFile+".svclog");
                    listener.TraceOutputOptions = TraceOptions.Timestamp | TraceOptions.Callstack;
                    traceSource.Attributes["propagateActivity"] = "true";
                    traceSource.Switch.ShouldTrace(TraceEventType.Verbose | TraceEventType.Start);
                    traceSource.Listeners.Add(listener);

                    // enable tracing  
                    type.GetProperty("Level", privateStaticMember).SetValue(null, SourceLevels.All, null);

                    Trace.AutoFlush = true;
                } 
              
                

            }
        }

        public int MaxReceivedMessageSize { get; set; }

        public event EventHandler<EventArgs<NetTcpBinding>> ConfigureNetTcpBinding;
        public event EventHandler<EventArgs<WSHttpBinding>> ConfigureWSHttpBinding;
        public event EventHandler<EventArgs<BasicHttpBinding>> ConfigureBasicHttpBinding;

        public virtual void ConfigureQuotas(XmlDictionaryReaderQuotas readerQuotas) {
            readerQuotas.MaxArrayLength = (int)Math.Max(readerQuotas.MaxArrayLength, this.MaxReceivedMessageSize);
            readerQuotas.MaxBytesPerRead = (int)Math.Max(readerQuotas.MaxBytesPerRead, this.MaxReceivedMessageSize);
            readerQuotas.MaxStringContentLength = (int)Math.Max(readerQuotas.MaxStringContentLength, this.MaxReceivedMessageSize);
            readerQuotas.MaxNameTableCharCount = (int)Math.Max(readerQuotas.MaxNameTableCharCount, this.MaxReceivedMessageSize);
            readerQuotas.MaxDepth = 1024;
        }

        public virtual void ConfigureBinding(NetTcpBinding binding) {
            ConfigureQuotas(binding.ReaderQuotas);
            binding.MaxReceivedMessageSize = Math.Max(binding.MaxReceivedMessageSize, this.MaxReceivedMessageSize);
            binding.MaxBufferPoolSize = (int) Math.Max(binding.MaxReceivedMessageSize, this.MaxReceivedMessageSize);
            binding.MaxBufferSize = (int) Math.Max(binding.MaxReceivedMessageSize, this.MaxReceivedMessageSize);
            
            binding.MaxConnections = 100;
            binding.PortSharingEnabled = false;
            binding.ReliableSession.Enabled = true;
            // binding.ReliableSession.InactivityTimeout = TimeSpan.MaxValue;
            if (ConfigureNetTcpBinding != null)
                ConfigureNetTcpBinding(this, new EventArgs<NetTcpBinding>(binding));
        }

        public virtual void ConfigureBinding(WSHttpBinding binding) {
            ConfigureQuotas(binding.ReaderQuotas);
            binding.MaxReceivedMessageSize = Math.Max(binding.MaxReceivedMessageSize, this.MaxReceivedMessageSize);
            binding.ReliableSession.Enabled = true;
            if (ConfigureWSHttpBinding != null)
                ConfigureWSHttpBinding(this, new EventArgs<WSHttpBinding>(binding));
        }

        public virtual void ConfigureBinding(BasicHttpBinding binding) {
            ConfigureQuotas(binding.ReaderQuotas);
            binding.MaxReceivedMessageSize = Math.Max(binding.MaxReceivedMessageSize, this.MaxReceivedMessageSize);
            if (ConfigureBasicHttpBinding != null)
                ConfigureBasicHttpBinding(this, new EventArgs<BasicHttpBinding>(binding));
        }

        public int MaxItemsInObjectGraph { get; set; }
        public virtual void ConfigureOperations(IEnumerable<OperationDescription> operations) {
            foreach (var operation in operations) {
                var behaviour = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                if (behaviour != null)
                    behaviour.MaxItemsInObjectGraph = MaxItemsInObjectGraph;
            }
        }

        public virtual void ConfigureHost(ServiceHost host) {
            foreach (var endpoint in host.Description.Endpoints)
                ConfigureOperations(endpoint.Contract.Operations);
        }

        public virtual void ConfigureFactory(ChannelFactory<T> factory) {
            ConfigureOperations(factory.Endpoint.Contract.Operations);
        }

       
    }
}