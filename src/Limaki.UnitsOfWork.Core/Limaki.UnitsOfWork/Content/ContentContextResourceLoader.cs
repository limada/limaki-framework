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
using Limaki.UnitsOfWork.Content.Usecases;
using Limaki.UnitsOfWork.Content.Usecases.Detectors;

namespace Limaki.UnitsOfWork.Content {

    public class ContentContextResourceLoader : ContextResourceLoader {

        public ContentContextResourceLoader (IContextResourceLoader resourceLoader) {
            this.ResourceLoader = resourceLoader;
        }

        public ContentContextResourceLoader () {
        }

        protected IContextResourceLoader ResourceLoader { get; set; }

        protected static bool Applied { get; set; }

        public override void ApplyResources (IApplicationContext context) {

            if (Applied)
                return;

            ResourceLoader?.ApplyResources (context);

            var streamContentIoPool = context.Pooled<ContentDetectorPool> ();
            streamContentIoPool.Add (new HtmlContentDetector ());
            streamContentIoPool.Add (new PdfContentDetector ());
            streamContentIoPool.Add (new TextContentDetector ());
            streamContentIoPool.Add (new ImageContentDetector ());
            streamContentIoPool.Add (new MarkdownContentDetector ());

            Applied = true;
        }
    }


}