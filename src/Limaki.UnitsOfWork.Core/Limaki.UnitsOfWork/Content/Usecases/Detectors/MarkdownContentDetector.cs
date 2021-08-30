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
using Limaki.UnitsOfWork.Usecases;

namespace Limaki.UnitsOfWork.Content.Usecases.Detectors {

    public class MarkdownContentDetector : ContentDetector {

        public static Guid Markdown { get; private set; } = new Guid ("e20eb44c-feb7-4d5a-86d1-afa95fe7e67d");

        static readonly ContentInfo[] infos = {
            new ContentInfo (
                "Markdown",
                Markdown,
                "md",
                // https://tools.ietf.org/html/draft-ietf-appsawg-text-markdown-06
                "text/markdown",
                CompressionTypes.BZip2
            )
        };

        public MarkdownContentDetector () : base (infos) { }
    }

}