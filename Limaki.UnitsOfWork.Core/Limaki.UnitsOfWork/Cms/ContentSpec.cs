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

namespace Limaki.UnitsOfWork.Cms {

    public class ContentSpec : IContentSpec {

        public ContentSpec (IEnumerable<ContentInfo> specs) {
            ContentSpecs = specs;
        }

        public virtual IEnumerable<ContentInfo> ContentSpecs { get; protected set; }

        protected bool? _streamHasMagics = null;
        public virtual bool SupportsMagics {
            get {
                if (!_streamHasMagics.HasValue) {
                    _streamHasMagics = ContentSpecs.Any (info => info.Magics != null && info.Magics.Any ());
                }
                return _streamHasMagics.Value;
            }
        }

        public virtual ContentInfo Find (string extension) => ContentSpecs.Find (extension);

        public virtual ContentInfo FindMime (string mime) => ContentSpecs.FindMime (mime);

        public virtual ContentInfo Find (Guid contentType) => ContentSpecs.Find (contentType);

        public virtual bool Supports (string extension) => ContentSpecs.Supports (extension);

        public virtual bool Supports (Guid contentType) => ContentSpecs.Supports (contentType);
    }
}
