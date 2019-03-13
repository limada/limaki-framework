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
using Limaki.Common;
using Limaki.Common.Text;
using LCHP = Limaki.Common.Text.HTML.Parser;

namespace Limaki.UnitsOfWork.Cms {

    public class HtmlContentDetector : ContentDetector {

        public static Guid HTML = ContentTypes.HTML;

        public static Guid XHTML { get; private set; } = new Guid ("48947e4e-3849-4573-8a93-86f89a0d0889");

        /// <summary>
        /// contains url of content
        /// on linux, this comes if pasted; on dragdrop, it don't com
        /// </summary>
        public static Guid MOZURL { get; private set; } = new Guid ("a50bda6d-a582-41b6-b891-0bea561637bb");

        public static Guid MOZINFO { get; private set; } = new Guid ("15ffdb43-1c72-41c8-bcd9-89f2b606e9d5");
        public static Guid MOZCONTEXT { get; private set; } = new Guid ("6918e840-7723-42c3-820d-10e5a4b31ef1");

        static ContentInfo[] infos = {

            new ContentInfo("HTML",HTML,"html","text/html",CompressionTypes.BZip2 ),
            new ContentInfo("XHTML", XHTML,"xhtml","application/xhtml+xml",CompressionTypes.BZip2),

            new ContentInfo("moz-url",MOZURL,"url","text/x-moz-url-priv", CompressionTypes.BZip2),
            new ContentInfo("moz-url",MOZINFO,"url","text/_moz_htmlinfo", CompressionTypes.BZip2),
            new ContentInfo("moz-url",MOZCONTEXT,"url","text/_moz_htmlcontext", CompressionTypes.BZip2),

        };

        public HtmlContentDetector () : base (infos){
            Digger = DiggFunc;
        }

        public override bool SupportsMagics => true;

        public override ContentInfo Find (Stream stream) {

            ContentInfo result = null;

            var buffer = stream.GetBuffer (2048);

            var s = (TextHelper.IsUnicode (buffer) ? Encoding.Unicode.GetString (buffer) : Encoding.ASCII.GetString (buffer)).ToLower ();
            if (
                s.Contains ("<!doctype html") ||
                s.Contains ("<html") ||
                s.Contains ("<head") ||
                s.Contains ("<body")) {
                result = ContentSpecs.First (t => t.ContentType == ContentTypes.HTML);
            }

            if (
                s.Contains ("<!doctype xhtml") ||
                s.Contains ("<xhtml")
                ) {
                result = ContentSpecs.First (t => t.ContentType == XHTML);
            }

            return result;
        }

        protected virtual Content<Stream> DiggFunc (Content<Stream> source, Content<Stream> sink) {
            if (!Supports (source.ContentType) || source.Data == null)
                return sink;

            var buffer = source.Data.GetBuffer ();
            if (sink.ContentType == MOZURL) {
                if (buffer != null) {
                    var desc = Encoding.Unicode.GetString (buffer);
                    sink.Source = desc;
                }
                return sink;
            }

            var s = (TextHelper.IsUnicode (buffer) ?
                Encoding.Unicode.GetString (buffer) :
                Encoding.UTF8.GetString (buffer));
            if (Fragment2Html (s, sink)) {
                buffer = sink.Data.GetBuffer ();
                s = Encoding.Default.GetString (buffer);
            } else {
                // TODO: find sink.Source
            }

            Digg (s, sink);
            return sink;
        }


        /// <summary>
        /// html dragdrop delivers fragments
        /// so extract the hmtl of the fragment
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sink"></param>
        protected virtual bool Fragment2Html (string source, Content<Stream> sink) {
            try {
                var subText = source.Between ("StartHTML:", "\r\n", 0);
                if (subText == null)
                    return false;
                if (!int.TryParse (subText, out var startIndex))
                    return false;
                subText = source.Between ("EndHTML:", "\r\n", 0);
                if (subText == null)
                    return false;
                if (!int.TryParse (subText, out var endIndex))
                    return false;
                if (startIndex != -1 && endIndex != -1) {
                    endIndex = Math.Max (endIndex, source.IndexOf ("</body>") + 7);
                    endIndex = Math.Min (source.Length, endIndex);
                    sink.Source = source.Between ("SourceURL:", "\r\n", 0);
                    sink.Data = new MemoryStream (Encoding.Default.GetBytes (source.Substring (startIndex, endIndex - startIndex)));
                }
                return true;
            } catch (Exception e) {
                throw e;
            }
        }

        public class Element {
            public string Name { get; set; }
            public bool Parsing { get; set; }
            public bool Parsed { get; set; }
            public int Starts { get; set; }
            public int Ends { get; set; }
            public string Text { get; set; }
            string _endTag = null;
            public string EndTag => _endTag ?? (_endTag = $"</{Name}>");
        }

        protected virtual void Digg (string source, Content<Stream> sink) {
            try {
                var parser = new LCHP.TagParser (source);
                var body = new Element { Name = "body", Text = "" };
                var elements = new Element[] {
                                new Element { Name = "title", Text = "" },
                                new Element { Name = "h1", Text = "" },
                                new Element { Name = "h2", Text = "" },
                                new Element { Name = "h3", Text = "" },
                                new Element { Name = "h4", Text = "" },
                                body,
                            };
                var plainText = "";
                parser.DoElement += stuff => {
                    var tag = stuff.Element.ToLower ();
                    foreach (var element in elements) {
                        if (!element.Parsing && !element.Parsed && stuff.State == LCHP.State.Name && tag == element.Name) {
                            element.Starts = stuff.TagPosition;
                            element.Parsing = true;
                        }
                    }

                };
                parser.DoTag += stuff => {
                    var tag = stuff.Tag.ToLower ();
                    var lineend = (tag == "</br>" || tag == "<br>" || tag == "<br/>" || tag == "<br />" || tag == "</div>" || tag == "</p>");
                    foreach (var element in elements) {
                        if (element.Parsing && !element.Parsed && stuff.State == LCHP.State.Endtag && tag == element.EndTag) {
                            element.Ends = stuff.Position;
                            element.Parsing = false;
                            element.Parsed = true;
                        }

                    }
                    if (body.Parsing && lineend) {
                        body.Parsing = false;
                        body.Parsed = true;
                    }

                };
                parser.DoText += stuff => {
                    var text = stuff.Text.ToString (stuff.Origin, stuff.Position - stuff.Origin);
                    foreach (var element in elements) {
                        if (element.Parsing)
                            element.Text += text;
                    }
                    if (body.Parsed || body.Parsing)
                        plainText += text;
                };

                var notEncoded = false;

                parser.NotEncoded = stuff => {
                    var co = stuff.Text.ToString (stuff.Position, 1);
                    var c = System.Net.WebUtility.HtmlEncode (co);
                    // HtmlEncode doesn't replace special unicode chars
                    if (co == c) {
                        c = $"&#{(int)c.ToCharArray (0, 1)[0]};";
                    }
                    stuff.Text.Remove (stuff.Position, 1);
                    stuff.Text.Insert (stuff.Position, c);
                    stuff.Position += c.Length - 1;
                    notEncoded = true;
                };

                parser.Parse ();
                if (notEncoded) {
                    source = parser.Stuff.Text.ToString ();
                    sink.Data.Dispose ();
                    sink.Data = source.AsAsciiStream ();
                }

                if (!body.Parsed) {
                    source = "<html><head></head><body>" + source + "</body></html>";
                    sink.Data.Dispose ();
                    sink.Data = source.AsAsciiStream ();
                    Digg (source, sink);
                }

                plainText = System.Net.WebUtility.HtmlDecode (plainText.Replace ("\n", " ").Replace ("\r", " ").Trim ());
                string description = null;
                foreach (var element in elements.Where (e => e.Parsed)) {
                    // TODO: replace unresolved unicode chars; see above
                    description = System.Net.WebUtility.HtmlDecode (element.Text.Replace ("\n", " ").Replace ("\r", " ").Trim ());
                    if (!string.IsNullOrWhiteSpace (description))
                        break;
                }
                if (description != null)
                    sink.Description = description;
                //if (description == plainText)
                //    sink.Data = null;

            } catch (Exception e) {
                throw e;
            }
        }
    }
}
