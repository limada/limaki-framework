/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.IO;

namespace Limaki.Common {

    public static class StreamExtensions {

        public static MemoryStream Clone (this Stream source) {
            return Clone (source, 1024 * 1024);
        }

        public static MemoryStream Clone (this Stream source, long maxbuffsize) {
            var result = new MemoryStream ();
            var bufferSize = Math.Min ((int)maxbuffsize, (int)source.Length);
            var buffer = new byte[bufferSize];
            var oldPos = source.Position;
            int readByte = 0;
            int position = 0;

            long endpos = source.Length - 1;
            while (position < endpos) {
                readByte = source.Read (buffer, 0, bufferSize);
                result.Write (buffer, 0, readByte);
                position += readByte;
            }
            source.Position = oldPos;
            result.Position = oldPos;
            return result;
        }
    }
}