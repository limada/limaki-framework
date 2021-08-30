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
    /// uses type Guid for Id's
    /// </summary>
    public class TridletGuid : Tridlet<Guid> {

        public TridletGuid () {
            CreateId = () => Guid.NewGuid ();

            MetaId.Type = new Guid ("9a88bf97-b079-49b4-99ae-246c5d315324");
            MetaId.TypeName = new Guid ("f1de3c59-3e50-4c7a-80ad-d9b2117587ad");
            MetaId.TypeMember = new Guid ("81d91fc5-45ea-4573-a384-c5b67da50da9");
            MetaId.Member = new Guid ("501cb0eb-5b36-4fb8-a5b8-6b96b2d10620");
            MetaId.MemberType = new Guid ("121f3d5d-eae0-480e-aee5-8241691956ff");
        }

    }

    public interface ITridleGuid<V> : ITridle<Guid, V> { }

    public interface IStringTridleGuid : ITridleGuid<string> { }
    public interface INumberTridleGuid : ITridleGuid<long> { }

    /// <summary>
    /// a tridle using Guid as for id's
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class TridleGuid<V> : Tridle<Guid, V>, ITridleGuid<V> { }

    public class StringTridleGuid : TridleGuid<string>, IStringTridleGuid { }
    public class NumberTridleGuid : TridleGuid<long>, INumberTridleGuid { }

}
