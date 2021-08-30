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
using System.Collections.Generic;
using System.Diagnostics;
using Limaki.Common.Reflections;

namespace Limaki.Common {

    public interface ILog {
        void Fatal (string msg, Exception ex);
        void Fatal (string msg);
        void Error (string msg, Exception ex);
        void Error (string msg);
        void Debug (string msg);
        void Info (string msg);
        void Warn (string msg);
        void Raw (string msg);
    }

    public class DelegateLog : ILog {

        public delegate void MsgHandler (string msg);
        public delegate void ExceptionMsgHandler (string msg, Exception ex);

        public event MsgHandler OnDebug;
        public event MsgHandler OnFatal;
        public event ExceptionMsgHandler OnExceptionFatal;
        public event MsgHandler OnError;
        public event ExceptionMsgHandler OnExceptionError;
        public event MsgHandler OnWarn;
        public event ExceptionMsgHandler OnExceptionWarn;
        public event MsgHandler OnInfo;
        public event MsgHandler OnRaw;

        public LogLevel LogLevel { get; set; }

        public virtual void Debug(string msg) {
            if (LogLevel.HasFlag(LogLevel.Debug)) OnDebug?.Invoke(msg);
        }

        public void Fatal(string msg, Exception ex) {
            if(LogLevel.HasFlag(LogLevel.Fatal)) OnExceptionFatal?.Invoke(msg, ex);
        }

        public void Fatal(string msg) {
            if(LogLevel.HasFlag(LogLevel.Fatal)) OnFatal?.Invoke(msg);
        }

        public virtual void Error(string msg, Exception ex) {
            if(LogLevel.HasFlag(LogLevel.Error)) OnExceptionError?.Invoke(msg, ex);
        }

        public virtual void Error(string msg) {
            if(LogLevel.HasFlag(LogLevel.Error)) OnError?.Invoke(msg);
        }

        public void Warn(string msg) {
            if(LogLevel.HasFlag(LogLevel.Warn)) OnWarn?.Invoke(msg);
        }

        public virtual void Info(string msg) {
            if(LogLevel.HasFlag(LogLevel.Info)) OnInfo?.Invoke(msg);
        }

        public virtual void Raw(string msg) {
            if(LogLevel.HasFlag(LogLevel.Raw))OnRaw?.Invoke(msg);
        }

        public virtual void Register (ILog log) {
            OnInfo += log.Info;
            OnDebug += log.Debug;
            OnWarn += log.Warn;
            OnError += log.Error;
            OnExceptionError += log.Error;
            OnFatal += log.Fatal;
            OnExceptionFatal += log.Fatal;
            OnRaw += log.Raw;
        }

        public virtual void UnRegister (ILog log) {
            OnInfo -= log.Info;
            OnDebug -= log.Debug;
            OnWarn -= log.Warn;
            OnError -= log.Error;
            OnExceptionError -= log.Error;
            OnFatal -= log.Fatal;
            OnExceptionFatal -= log.Fatal;
            OnRaw -= log.Raw;
        }

    }

    public class TypedLog {

        public Type Type { get; set; }

        protected virtual string ErrorMsg (string msg, Exception ex) => $"Error<{Type.FriendlyClassName()}> {msg} Exception {ex.Message} | {DateTime.Now}";
        protected virtual string ErrorMsg (string msg) => $"Error<{Type.FriendlyClassName ()}> {msg} | {DateTime.Now}";

        protected virtual string FatalMsg(string msg, Exception ex) => $"Fatal<{Type.FriendlyClassName()}> {msg} Exception {ex.Message} | {DateTime.Now}";
        protected virtual string FatalMsg(string msg) => $"Fatal<{Type.FriendlyClassName()}> {msg} | {DateTime.Now}";

        protected virtual string DebugMsg (string msg) => $"Debug<{Type.FriendlyClassName ()}> {msg} | {DateTime.Now}";
        protected virtual string InfoMsg (string msg) => $"Info<{Type.FriendlyClassName ()}> {msg} | {DateTime.Now}";
        protected virtual string WarnMsg(string msg) => $"Warning<{Type.FriendlyClassName()}> {msg} | {DateTime.Now}";

    }

    public class TraceLog : TypedLog, ILog {

        public virtual void Fatal(string msg, Exception ex) => Trace.WriteLine(FatalMsg(msg, ex));
        public virtual void Fatal(string msg) => Trace.WriteLine(FatalMsg(msg));

        public virtual void Error(string msg, Exception ex) => Trace.WriteLine(ErrorMsg(msg, ex));
        public virtual void Error(string msg) => Trace.WriteLine(ErrorMsg(msg));

        public virtual void Warn(string msg) => Trace.WriteLine(WarnMsg(msg));

        public virtual void Debug (string msg) {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(DebugMsg(msg));
#else
            Trace.WriteLine (DebugMsg (msg));
#endif
        }

        public virtual void Info(string msg) => Trace.WriteLine(InfoMsg(msg));

        public virtual void Raw(string msg) => Trace.WriteLine(msg);

    }

    public class ConsoleLog : TypedLog, ILog {

        public virtual void Fatal(string msg, Exception ex) => Console.WriteLine(FatalMsg(msg, ex));
        public virtual void Fatal(string msg) => Console.WriteLine(FatalMsg(msg));

        public virtual void Error(string msg, Exception ex) => Console.WriteLine(ErrorMsg(msg, ex));
        public virtual void Error(string msg) => Console.WriteLine(ErrorMsg(msg));

        public virtual void Warn(string msg) => Console.WriteLine(WarnMsg(msg));

        public virtual void Debug(string msg) => Console.WriteLine(DebugMsg(msg));

        public virtual void Info(string msg) => Console.WriteLine(InfoMsg(msg));

        public virtual void Raw(string msg) => Console.WriteLine(msg);
    }

    [Flags]
    public enum LogLevel {
        none = 0,
        Raw = 1 << 0,
        Debug = 1 << 1,
        Info = 1 << 2,
        Warn = 1 << 3,
        Error = 1 << 4,
        Fatal = 1 << 5,
        All = Raw | Debug | Info | Warn | Error | Fatal
    }

    public class Logger {

        protected DelegateLog _listener = new DelegateLog ();
        protected IDictionary<Type, ILog> _logs = new Dictionary<Type, ILog> ();

        public virtual DelegateLog Listener {
            get => _listener;
            set => _listener = value;
        }

        public bool Trace { get; set; }
        #if TRACE
        = true;
        #endif

        public bool Console { get; set; }
#if !TRACE
        = true;
#endif
        public LogLevel LogLevel { get; set; }
#if DEBUG
                 = LogLevel.All;
#else
                = LogLevel.All & ~LogLevel.Debug;
#endif
        public ILog Log<T>() => Log(typeof(T));

        public virtual ILog Log (Type type) {

            if (!_logs.TryGetValue (type, out ILog result)) {
                var llog = new DelegateLog {
                    LogLevel = LogLevel
                };

                if (Trace) {
                    var log = new TraceLog { Type = type };
                    llog.Register (log);
                }
                if (Console) {
                    var log = new ConsoleLog { Type = type };
                    llog.Register (log);
                }

                lock (_logs) {
                    _logs[type] = llog;
                }
                llog.Register (Listener);
                result = llog;
            }
            return result;
        }
    }
}
