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
    public class SanityCheckResult
    {
        public GuidFlags Flags { get; set; }
        public Guid Status { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Flags} | {SanityCheckFlags.Instance.NameOf(Status)} | {Message}";
        }
    }
}

