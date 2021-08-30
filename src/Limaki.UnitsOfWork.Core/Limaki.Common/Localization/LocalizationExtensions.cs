/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2017 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using System.IO;

namespace Limaki {

    public static class Localization {

        public static IDictionary<string, string> LocalisationDictionary = new Dictionary<string, string> ();

        public static ISet<string> MissingLocalisations = new HashSet<string> ();

        public static string DictionaryFilename { get; set; } = "GermanDict.csv";
        public static byte [] DictionaryResource { get; set; }

        static Localization () {
            Init ();
        }

        public static void Add (string filename) {

            if (!File.Exists (filename))
                return;

            using (var reader = new StreamReader (filename)) {
                Add (reader);
            }
        }

        public static void Add (byte [] resource) {

            if (resource == null)
                return;

            Add (new StreamReader (new MemoryStream (resource)));
        }

        public static void Add (TextReader reader) {

            var line = reader.ReadLine ();
            while (line != null) {
                if (!string.IsNullOrEmpty (line) && line.StartsWith ("\"")) {
                    var entry = line.Split (new string [] { "\",\"" }, StringSplitOptions.None);
                    LocalisationDictionary [entry [0].TrimStart ('"')] = entry [1].TrimEnd ('"');
                }
                line = reader.ReadLine ();
            }
            reader.Dispose ();
        }

        public static void Init () {

            if (File.Exists (DictionaryFilename)) {

                LocalisationDictionary.Clear ();

                Add (new StreamReader (DictionaryFilename));

            }

            if (DictionaryResource != null) {
                Add (DictionaryResource);
            }

        }

        public static string ___ (string s) {
            if (s == null)
                return s;
            var st = s;
            if (LocalisationDictionary.TryGetValue (s, out st))
                return st;
            else {
                MissingLocalisations.Add (s);
            }
            return s;
        }

        public static string __ (FormattableString s) {
            if (s == null)
                return null;
            var format = s.Format;
            if (LocalisationDictionary.TryGetValue (s.Format, out format))
                return string.Format (format, s.GetArguments ());
            else {
                MissingLocalisations.Add (s.Format);
            }
            return s.ToString ();
        }

    }


}