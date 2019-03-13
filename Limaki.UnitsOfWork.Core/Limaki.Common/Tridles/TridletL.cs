/*
 * Tridles 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2015 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;

namespace Limaki.Common.Tridles {

    /// <summary>
    /// uses type long for Id's
    /// </summary>
    public class TridletL : Tridlet<long> {

        public TridletL () {
            CreateId = () => Limaki.Common.Isaac.Long;

            MetaId.Type = unchecked((long)0xEDAA45C4C48EF14F);
            MetaId.TypeName = unchecked((long)0xCC0EE7B5FD2CAC99);
            MetaId.TypeMember = unchecked((long)0x10B5B566F55E8520);
            MetaId.Member = unchecked((long)0xBEB8D657E634B222);
            MetaId.MemberType = unchecked((long)0x99D74D2FE6E2C9B9);
        }

    }

    /// <summary>
    /// a tridle using long as for id's
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class TridleL<V> : Tridle<long, V> { }

    public class StringTridleL : TridleL<string> { }
    public class NumberTridleL : TridleL<long> { }
}
    
