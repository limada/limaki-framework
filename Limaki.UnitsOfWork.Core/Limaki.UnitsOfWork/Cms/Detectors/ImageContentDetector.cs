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
using System.IO;
using System.Text;
using Limaki.Common;
using Limaki.Imaging;

namespace Limaki.UnitsOfWork.Cms {

    public class ImageContentDetector : ContentDetector {

        public static Guid PNG => ContentTypes.PNG;
        public static Guid TIF => ContentTypes.TIF;
        public static Guid JPG => ContentTypes.JPG;

        public static Guid GIF => ContentTypes.GIF;
        public static Guid BMP => ContentTypes.BMP;
        public static Guid DIB => ContentTypes.DIB;

        static readonly ContentInfo[] infos = {
            // place prefered formats first
            new ContentInfo (
                "PNG image",
                PNG,
                "png",
                "image/png",
                CompressionTypes.NeverCompress,
                new[] {
                    new Magic (new byte[] {0x89, 0x50, 0x4e, 0x47}, 0)
                }),

            new ContentInfo (
                "Tagged Image File Format",
                TIF,
                "tif",
                "image/tiff",
                CompressionTypes.BZip2,
                new[] {
                    new Magic (new byte[] {0x49, 0x49, 0x2A, 0x00}, 0),
                    new Magic (new byte[] {0x4D, 0x4D, 0x00, 0x2A}, 0)
                }),

            new ContentInfo (
                "GIF image",
                GIF,
                "gif",
                "image/gif",
                CompressionTypes.NeverCompress,
                new[] {
                    new Magic (Encoding.ASCII.GetBytes (@"GIF"), 0)
                }),

            new ContentInfo (
                "Device Independent Bitmap",
                DIB,
                "dib",
                "DeviceIndependentBitmap", // TODO:look for dib mime, or register with MimeFingerPrints
                CompressionTypes.BZip2,
                null),

            new ContentInfo (
                "Bitmap",
                BMP,
                "bmp",
                "image/bmp", // TODO:look for bmp mime, or register with MimeFingerPrints
                CompressionTypes.BZip2,
                new[] {
                    new Magic (new byte[] {0x42, 0x4D}, 0)
                }),

            new ContentInfo (
                "JPEG image",
                JPG,
                "jpg",
                "image/jpeg",
                CompressionTypes.NeverCompress,
                new[] {
                    new Magic (new byte[] {0xff, 0xd8}, 0),
                    new Magic (new[] {377, 330, 377}.BytesOfArray (), 0)
                }),
        };

        public ImageContentDetector () : base (infos) { Digger = DiggFunc; }

        protected virtual Content<Stream> DiggFunc (Content<Stream> source, Content<Stream> sink) {
            if (!Supports (source.ContentType))
                return sink;

            if (source.ContentType == ContentTypes.DIB) {
                var bmp = new BitmapFromDibStream (source.Data);
                var sinkStream = bmp.Clone (bmp.Length);
                sink.Data = sinkStream;
                sink.ContentType = ContentTypes.BMP;
            }

            return sink;
        }
    }

}