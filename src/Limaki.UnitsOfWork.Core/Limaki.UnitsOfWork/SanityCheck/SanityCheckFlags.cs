/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 - 2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using Limaki.Common.Collections;

namespace Limaki.UnitsOfWork.SanityCheck
{

    public class SanityCheckFlags : GuidFlags
    {
        public SanityCheckFlags() { }

        public SanityCheckFlags(params Guid[] flags) { Add(flags); }

        public static SanityCheckFlags operator |(SanityCheckFlags c1, Guid g) => With(c1, g);
        public static implicit operator SanityCheckFlags(Guid value) => new SanityCheckFlags(value);
        public static SanityCheckFlags __() => new SanityCheckFlags();
        public static new SanityCheckFlags __(params Guid[] flags) => new SanityCheckFlags(flags);

        public static SanityCheckFlags Instance = __();

        public static Guid Ok { get; private set; } = new Guid("0329dde2-120e-495e-8700-7c7b7cf63802");
        public static Guid Error { get; private set; } = new Guid("3124b81c-49dd-433a-a554-5692367dea94");
        public static Guid Warning { get; private set; } = new Guid("b583cf71-6edf-4a64-8085-72fbadf7b3d9");

        public static Guid Info { get; private set; } = new Guid("0468bd80-c785-4326-b978-e5b218b63091");

        public static Guid TestCase { get; private set; } = new Guid("5f645691-0cca-4427-9875-c4ceff2f6cff");

        public static Guid Verbose { get; private set; } = new Guid("a1746b5a-ccaa-4ecd-b4ad-651fa969bdae");
    }
}

