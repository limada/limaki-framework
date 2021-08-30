/*
 * Limada
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2006-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

namespace Limaki.UnitsOfWork.Content {

    public class Magic {

        public Magic () { }

        public Magic (byte[] magic, int offset) {
            Offset = offset;
            Bytes = magic;
            OffsetIsRange = false;
        }

        public int Offset { get; set; }

        /// <summary>
        /// somewhere between 0 and offset bytes are to be found
        /// </summary>
        public bool OffsetIsRange { get; set; }
        public byte[] Bytes { get; set; }
    }
}
