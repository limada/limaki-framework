﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// this is a copy from: 
//  EntityFrameworkCore/src/EFCore/ValueGeneration/SequentialGuidValueGenerator.cs 

using System;
using System.Threading;

namespace Limaki.Common {

    public class SequentialGuidValueGenerator {

        private long _counter = DateTime.UtcNow.Ticks;

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <para>The change tracking entry of the entity for which the value is being generated.</para>
        /// <returns> The value to be assigned to a property. </returns>
        public Guid Next () {
            var guidBytes = Guid.NewGuid ().ToByteArray ();
            var counterBytes = BitConverter.GetBytes (Interlocked.Increment (ref _counter));

            if (!BitConverter.IsLittleEndian) {
                Array.Reverse (counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return new Guid (guidBytes);
        }

        /// <summary>
        ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
        ///     always returns false, meaning the generated values will be saved to the database.
        /// </summary>
        public bool GeneratesTemporaryValues => false;
    }
}

