/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Limaki.Common.Reflections {
    
    public class MemberInfoEx : TypeInfoEx {

        public PropertyInfo PropertyInfo { get; set; }

        public override Type Type { get { return PropertyInfo.PropertyType; } set { } }

        public override string Name => PropertyInfo.Name;

        public RelationAttribute Relation {
            get {
                var a = PropertyInfo.GetCustomAttributes<RelationAttribute> ().FirstOrDefault ();
                //return (a?.Name, a?.ThisKey, a?.OtherKey);
                return a;
            }
        }

        public bool IsRelation => PropertyInfo.GetCustomAttributes<RelationAttribute> ().Any ();

        public bool CanDisplay => PropertyInfo.GetCustomAttributes<DisplayAttribute> ().Any ();

        public bool IsDataMember => PropertyInfo.GetCustomAttributes<DataMemberAttribute> ().Any ();

        public bool IsPrimitive => PropertyInfo.PropertyType.IsPrimitive || Type == typeof (string) || Type == typeof (Decimal);

        public bool IsReadonly => !PropertyInfo.CanWrite;

        public bool IsKey => PropertyInfo.GetCustomAttributes<KeyAttribute> ().Any ();

        public bool IsEnumerable => typeof (IEnumerable).IsAssignableFrom (Type) && Type != typeof (string);

        public string DataFormat => PropertyInfo.GetCustomAttributes<DisplayFormatAttribute> ().FirstOrDefault ()?.DataFormatString ?? null;

    }
}
