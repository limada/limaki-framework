/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;

namespace System {

    public static class SystemExtensions {

        public static string TickCounts (this int startTime) => ((Environment.TickCount - startTime) / 1000d).ToString ();

        public static string ExceptionMessage (this Exception ex, string label, bool stackTrace = true) {
            var msg = $"{label} : {ex?.Message}\n{(stackTrace ? ex?.StackTrace : "")}";

            if (ex?.InnerException != null)
                msg += $"\n{nameof(Exception.InnerException)} : {ex.InnerException?.Message}\n{(stackTrace ? ex.InnerException?.StackTrace : "")}";

            return msg;

        }

    }

}