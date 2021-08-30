/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.IO;
using System.Reflection;
using Limaki.Common;

namespace Limaki.UnitsOfWork.Service.Server {

    public class ServiceBase : IServiceBase {

        public StringWriter LogWriter = new StringWriter ();

        protected ILog _logger = null;
        protected virtual ILog Log => _logger ?? (_logger = Registry.Pool.TryGetCreate<Logger> ().Log (this.GetType ()));

        #region Exception-Handling

        protected string MethodName (MethodBase method) {
            if (method != null)
                return method.ToString ();
            else
                return "<null>";
        }

        public virtual void Catch (Exception ex) {
            var caller = MethodName (new System.Diagnostics.StackTrace ().GetFrame (2).GetMethod ());
            Catch (ex, caller);
        }

        public virtual void Catch (Exception ex, string msg) {
            Log.Error (msg, ex);
            if (this.LogWriter != null) {
                Log.Error (this.LogWriter.ToString ());
                this.LogWriter = new StringWriter ();
            }
            CatchBeforeFault?.Invoke (this, ex, msg);
            throw ex;
            //throw new FaultException<string>(msg, new FaultReason(ex.GetType().Name), new FaultCode(ex.GetType().Name), "");
            // throw new FaultException<string> (msg, ex.Message);
        }

        public Action<IServiceBase, Exception, string> CatchBeforeFault { get; set; }
       
        protected T AssertNotNull<T> (T result) {
            var caller = MethodName (new System.Diagnostics.StackTrace ().GetFrame (2).GetMethod ());
            return AssertNotNull (result, caller);
        }

        protected T AssertNotNull<T> (T result, string msg) {
            if (result == null)
                Catch (new NullReferenceException (), msg);
            return result;
        }

        #endregion

        public virtual bool Ping () {
            var msg = this.GetType ().Name + ".Ping";
            Log.Debug (msg);
            return true;
        }

        public virtual string Test () {
            var msg = this.GetType ().Name + ".Test";
            Log.Debug (msg);
            return null;
        }

        public virtual byte [] Resource (string name) {
            name = @"Resources\" + name;
            if (File.Exists (name)) {
                using (var reader = new FileStream (name, FileMode.Open)) {
                    var result = new byte [reader.Length];
                    reader.Read (result, 0, (int)reader.Length);
                    return result;
                }
            }
            return null;
        }

        protected static string _serverVersion = null;
        public virtual string ServerVersion () {
            return _serverVersion ?? (_serverVersion = Assembly.GetAssembly (this.GetType ()).GetName ().Version.ToString (3));
        }

    }
    
}
