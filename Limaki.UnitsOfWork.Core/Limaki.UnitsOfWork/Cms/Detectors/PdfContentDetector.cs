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
using System.Text;

namespace Limaki.UnitsOfWork.Cms {

    public class PdfContentDetector : ContentDetector {

        public static Guid PdfContentType { get; private set; } = new Guid ("e8efdfbf-8c83-4f0e-b7dd-4a5c7318696f");

        public PdfContentDetector () : base (
            new[] {
                new ContentInfo (
                    "Portable Document Format",
                    PdfContentType,
                    "pdf",
                    "application/pdf",
                    CompressionTypes.None,
                    new[] {new Magic (Encoding.ASCII.GetBytes (@"%PDF-"), 0)}
                )
            }
        ) { }
    }

}