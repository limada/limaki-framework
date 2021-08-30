using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Limaki.Common.Reflections {
    
    public class MemberInfo : TypeInfo {

        public PropertyInfo PropertyInfo { get; set; }

        public override Type Type { get { return PropertyInfo.PropertyType; } set { } }

        public override string Name => PropertyInfo.Name;

        public RelationAttribute Association {
            get {
                var a = PropertyInfo.GetCustomAttributes<RelationAttribute> ().FirstOrDefault ();
                //return (a?.Name, a?.ThisKey, a?.OtherKey);
                return a;
            }
        }

        public bool IsAssociation => PropertyInfo.GetCustomAttributes<RelationAttribute> ().Any ();

        public bool CanDisplay => PropertyInfo.GetCustomAttributes<DisplayAttribute> ().Any ();

        public bool IsDataMember => PropertyInfo.GetCustomAttributes<DataMemberAttribute> ().Any ();

        public bool IsPrimitive => PropertyInfo.PropertyType.IsPrimitive || Type == typeof (string) || Type == typeof (Decimal);

        public bool IsReadonly => !PropertyInfo.CanWrite;

        public bool IsKey => PropertyInfo.GetCustomAttributes<KeyAttribute> ().Any ();

        public bool IsEnumerable => typeof (IEnumerable).IsAssignableFrom (Type) && Type != typeof (string);

        public string DataFormat => PropertyInfo.GetCustomAttributes<DisplayFormatAttribute> ().FirstOrDefault ()?.DataFormatString ?? null;

    }
}
