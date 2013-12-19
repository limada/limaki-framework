/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2011 Lytico
 *
 * http://www.limada.org
 * 
 */

using System.IO;
namespace Limaki.Common {
    public class FileHelpers {
        public static byte[] RawFile(string file) {
            var filestream = File.OpenRead(file);
            var buff = new byte[filestream.Length];
            filestream.Read(buff, 0, (int)filestream.Length);
            filestream.Close();
            return buff;
        }
    }
}