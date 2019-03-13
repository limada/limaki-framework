using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System {

    public static class EnumExtensions {

        public static string ToDescription (this System.Enum value) {
            if (value == null)
                return null;
            var fi = value.GetType ().GetField (value.ToString ());
            var attributes = (DisplayAttribute[])fi.GetCustomAttributes (typeof (DisplayAttribute), false);
            return attributes.Length > 0 ? attributes[0].Name : value.ToString ();
        }

        public static bool Contains<E> (string value) where E : struct, System.Enum {
            var it = new E ();
            return typeof (E).GetFields ().Any (f => f.GetValue (it).ToString () == value);
        }

        public static E FromDescription<E> (this E it, string value) where E : struct, System.Enum {
            if (value == null)
                return default;
            var f = typeof (E).GetFields ()
                              .Select (fi => (fi, fi.GetCustomAttributes (typeof (DisplayAttribute), false).OfType<DisplayAttribute> ().FirstOrDefault ()?.Name))
                              .Where (fi => fi.Name == value)
                              .FirstOrDefault ();
            if (f != default) {
                return (E)f.fi.GetValue (it);
            }
            System.Enum.TryParse (value, out E result);
            return result;

        }
    }


}
