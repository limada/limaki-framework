using System;
namespace Limaki.Common {
    public class BooleanFormatProvider : IFormatProvider, ICustomFormatter {
        public object GetFormat(Type formatType) {
            if (formatType == typeof(ICustomFormatter)) {
                return this;
            } else {
                return null;
            }
        }
        public string Format(string format, object arg, IFormatProvider formatProvider) {
            if (!(arg is bool)) {
                throw new ArgumentException("Must be a boolean type", "arg");
            }

            var value = (bool)arg;
            format = format == null ? string.Empty : format.Trim();
            return value ? format : string.Empty;
        }
    }
}