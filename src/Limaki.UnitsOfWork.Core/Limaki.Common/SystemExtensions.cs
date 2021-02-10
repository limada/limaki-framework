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