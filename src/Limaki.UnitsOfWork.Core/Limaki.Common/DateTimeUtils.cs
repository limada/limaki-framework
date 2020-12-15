using System;

namespace Limaki.Common {

    public static class DateTimeUtils {
        public static DateTime TruncateMilliseconds (this DateTime it) => new DateTime (it.Year, it.Month, it.Day, it.Hour, it.Minute, it.Second);
        public static DateTime Max (this DateTime it, DateTime other) => it > other ? it : other;

    }

}