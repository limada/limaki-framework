/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018 Lytico
 *
 * http://www.limada.org
 * 
 */

using Limaki.Common.IOC;
using Limaki.UnitsOfWork.Cms;

namespace Limaki.UnitsOfWork {

    public class UnitsOfWorkContextResourceLoader : ContextResourceLoader {

        protected static bool Applied { get; set; }

        public override void ApplyResources (IApplicationContext context) {
            if (Applied)
                return;

            context.Factory.Add<ICompressionWorker, CompressionWorker> ();

            var streamContentIoPool = context.Pooled<ContentDetectorPool> ();
            streamContentIoPool.Add (new HtmlContentDetector ());
            streamContentIoPool.Add (new PdfContentDetector ());
            streamContentIoPool.Add (new TextContentDetector ());

            Applied = true;
        }
    }
}