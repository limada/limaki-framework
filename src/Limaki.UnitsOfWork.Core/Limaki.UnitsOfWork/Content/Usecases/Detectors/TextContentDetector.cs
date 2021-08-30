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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Limaki.Common;
using Limaki.Common.Text;
using Limaki.UnitsOfWork.Usecases;

namespace Limaki.UnitsOfWork.Content.Usecases.Detectors {

    public class TextContentDetector : ContentDetector {

        public static Guid Text { get; private set; } = ContentTypes.Text;

        public static Guid ASCII { get; private set; } = ContentTypes.ASCII;

        static readonly ContentInfo[] infos = {
            new ContentInfo (
                "Text",
                Text,
                "txt",
                "text/plain",
                CompressionTypes.BZip2,
                null),
            new ContentInfo (
                "Text",
                Text,
                "txt",
                "text",
                CompressionTypes.BZip2,
                null),
            new ContentInfo (
                "ASCII",
                ASCII,
                "txt",
                "text/plain",
                CompressionTypes.BZip2,
                null),
        };

        public TextContentDetector () : base (infos) { Digger = DiggFunc; }

        public override ContentInfo Find (Stream stream) =>
            TextHelper.IsUnicode (stream.GetBuffer ()) ? 
            ContentSpecs.First (t => t.ContentType == ContentTypes.Text) :
            ContentSpecs.First (t => t.ContentType == ContentTypes.ASCII);

        protected virtual Content<Stream> DiggFunc (Content<Stream> source, Content<Stream> sink) {
            if (!Supports (source.ContentType) || source.Data == null)
                return sink;
            var buffer = source.Data.GetBuffer (2048);
            var s = TextHelper.IsUnicode (buffer) ? 
                    Encoding.Unicode.GetString (buffer) : 
                    Encoding.ASCII.GetString (buffer);

            // find lines
            var rx = new Regex ("\r\n|\n|\r|\n|\f");
            var matches = rx.Matches (s);

            // extract first line
            if (matches.Count > 0) {
                sink.Description = s.Substring (0, matches[0].Index);
            } else {
                // TODO: if there is only one line,don't use a sink stream!!
                sink.Description = s;
                sink.Data = null;
            }

            return sink;
        }
    }

}