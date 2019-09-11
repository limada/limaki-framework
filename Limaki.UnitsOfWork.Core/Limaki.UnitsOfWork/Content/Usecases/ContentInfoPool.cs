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

using System;
using System.Collections.Generic;
using System.Linq;
using Limaki.Common.Linqish;

namespace Limaki.UnitsOfWork.Content.Usecases {

    public class ContentInfoPool : IEnumerable<ContentInfo>, IContentInfoDetector {

        private ISet<ContentInfo> _contentInfos = new HashSet<ContentInfo> ();

        public void AddRange (IEnumerable<ContentInfo> contentInfos) => contentInfos.ForEach (ci => _contentInfos.Add (ci));

        public void Add (ContentInfo contentInfo) => _contentInfos.Add (contentInfo);
        public void Remove (ContentInfo contentInfo) => _contentInfos.Remove (contentInfo);
        public void RemoveRange (IEnumerable<ContentInfo> contentInfos) => contentInfos.ForEach (ci => _contentInfos.Remove (ci));

        public IEnumerator<ContentInfo> GetEnumerator () => _contentInfos.GetEnumerator ();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () => this.GetEnumerator ();

        public ContentInfo Find (string extension) => this.FirstOrDefault (s => s.Extension == extension);

        public ContentInfo FindMime (string mime) => this.FirstOrDefault (s => s.MimeType == mime);

        public ContentInfo Find (Guid contentType) => this.FirstOrDefault (s => s.ContentType == contentType);

        public bool Supports (string extension) => Find (extension) != null;

        public bool Supports (Guid contentType) => Find (contentType) != null;

        public IEnumerable<ContentInfo> ContentSpecs => this;
    }
}
