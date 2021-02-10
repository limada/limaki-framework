/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2010-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.Common.Reflections;

namespace Limaki.Common {

    public enum MessageType {
        OK,
        RetryCancel
    }

    public interface IExceptionHandler {
        void Catch ( Exception e );
        void Catch(Exception e, MessageType messageType);
    }

    public class ThrowingExceptionHandler:IExceptionHandler {
        public virtual void Catch(Exception e) {
            throw e;
        }
        public virtual void Catch(Exception e, MessageType messageType) {
            throw e;
        }
    }

    public class LoggingExceptionHandler : IExceptionHandler {

        static ILog _log = null;

        public virtual ILog Log {
            get {
                if (_log == null) {

                    try {
                        _log = Registry.Pool.TryGetCreate<Logger> ().Log (this.GetType ());
                    } finally {
                        _log = new Logger ().Log (GetType ());
                    }
                }

                return _log;
            }
        }

        public static string ExceptionMessage (Exception ex)
            => ex.ExceptionMessage ($"Error<{ex?.TargetSite.DeclaringType.FriendlyClassName ()}>\t{ex?.TargetSite.Name}");

        public virtual void Catch (Exception e) {
            Log.Raw (ExceptionMessage (e));
        }

        public virtual void Catch (Exception e, MessageType messageType) {
            Log.Raw (ExceptionMessage (e));
        }

    }

}