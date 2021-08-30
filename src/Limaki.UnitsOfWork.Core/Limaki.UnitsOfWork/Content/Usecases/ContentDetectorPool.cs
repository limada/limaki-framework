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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Limaki.UnitsOfWork.Content.Usecases {

    public class ContentDetectorPool : IEnumerable<ContentDetector> {

        List<ContentDetector> _contentDetectors = new List<ContentDetector> ();

        public ContentInfoPool ContentInfos { get; } = new ContentInfoPool ();

        public ContentDetectorPool Add (ContentDetector c) {
            _contentDetectors.Add (c);
            ContentInfos.AddRange (c.ContentSpecs);
            return this;
        }

        public virtual void Remove (ContentDetector c) {
            _contentDetectors.Remove (c);
            ContentInfos.RemoveRange (c.ContentSpecs);
        }

        public virtual ContentInfo FindInfo (Stream stream) {
            foreach (var d in _contentDetectors) {
                var i = d.Find (stream);
                if (i != default)
                    return i;
            }
            return default;
        }

        public virtual ContentDetector Find (Stream stream) => _contentDetectors.FirstOrDefault (d => d.Supports (stream));

        public IEnumerator<ContentDetector> GetEnumerator () => _contentDetectors.GetEnumerator ();

        IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();
    }
}
