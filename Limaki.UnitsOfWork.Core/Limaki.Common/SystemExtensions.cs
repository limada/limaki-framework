using System;

namespace System {

    public static class SystemExtensions {

        public static string TickCounts (this int startTime) => ((Environment.TickCount - startTime) / 1000d).ToString ();

        public static string ExceptionMessage (this Exception ex, string label) {
            var msg = $"{label} : {ex}\n{ex?.StackTrace}";
            if (ex?.InnerException != null)
                msg += $"\n{nameof (Exception.InnerException)} : {ex.InnerException}\n{ex.InnerException?.StackTrace}";
            return msg;

        }
    }


}
