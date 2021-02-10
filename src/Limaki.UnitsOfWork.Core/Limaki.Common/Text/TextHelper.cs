/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2013 Lytico
 *
 * http://www.limada.org
 * 
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Limaki.Common.Text {

    public static class TextHelper {

        public static bool Contains(this StringBuilder haystack, string needle) => haystack.IndexOf(needle) != -1;

        public static int IndexOf(this StringBuilder haystack, string needle) => IndexOf(haystack, needle, 0);

        /// <summary>
        /// Simple implementation of the Knuth–Morris–Pratt algorithm that only cares about ordinal matches 
        /// (no case-folding, no culture-related collation, just a plain codepoint to codepoint match). 
        /// It has some initial Θ(m) overhead where m is the length of the word sought, and then finds in Θ(n) 
        /// where n is the distance to the word sought, or the length of the whole string-builder if it isn't there. 
        /// This beats the simple char-by-char compare which is Θ((n-m+1) m) 
        /// (Where O() notation describes upper-bounds, Θ() describes both upper and lower bounds).
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/12261344/fastest-search-method-in-stringbuilder</remarks>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static int IndexOf (this StringBuilder haystack, string needle, int startIndex) {
            if (haystack == null || needle == null)
                throw new ArgumentNullException ();
            if (needle.Length == 0)
                return 0; //empty strings are everywhere!
            if (needle.Length == 1) { //can't beat just spinning through for it

                char c = needle[0];
                for (int idx = startIndex; idx != haystack.Length; ++idx)
                    if (haystack[idx] == c)
                        return idx;
                return -1;
            }
            int m = startIndex;
            int i = 0;
            int[] T = KMPTable (needle);
            while (m < haystack.Length - 1) {
                if (needle[i] == haystack[m + i]) {
                    if (i == needle.Length - 1)
                        return m == needle.Length ? -1 : m; //match -1 = failure to find conventional in .NET
                    ++i;
                } else {
                    m = m + i - T[i];
                    i = T[i] > -1 ? T[i] : 0;
                }
            }
            return -1;
        }

        private static int[] KMPTable (string sought) {
            int[] table = new int[sought.Length];
            int pos = 2;
            int cnd = 0;
            table[0] = -1;
            table[1] = 0;
            while (pos < table.Length)
                if (sought[pos - 1] == sought[cnd])
                    table[pos++] = ++cnd;
                else if (cnd > 0)
                    cnd = table[cnd];
                else
                    table[pos++] = 0;
            return table;
        }

        public static string Between (this StringBuilder it, string start, string end, int startIndex = 0) {
            int posStart = it.IndexOf (start, startIndex);
            if (posStart == -1) return null;
            posStart += start.Length;
            int posEnd = it.IndexOf (end, posStart);
            if (posEnd == -1) return null;
            return it.ToString (posStart, posEnd - posStart);
        }

        public static string Between (this string it, string start, string end, int startIndex = 0) {
            int posStart = it.IndexOf (start, startIndex);
            if (posStart == -1) return null;
            posStart += start.Length;
            int posEnd = it.IndexOf (end, posStart);
            if (posEnd == -1) return null;
            return it.Substring (posStart, posEnd - posStart);
        }


        public static bool IsUnicode (byte[] buffer) {
            var isUnicode = 0;
            for (int i = 1; i < buffer.Length; i += 2) {
                if (buffer[i] == 0)
                    isUnicode++;
            }
            return isUnicode > buffer.Length / 10;

        }

        public static string ReplaceLeading (this string it, char leadChar, string replaceWith) {
            if (string.IsNullOrEmpty (it))
                return it;
            var lastLead = -1;
            for (var i = 0; i < it.Length; i++) {
                if (it[i] != leadChar) {
                    lastLead = i;
                    break;
                }
            }
            if (lastLead == -1)
                return it;
            return string.Concat (Enumerable.Repeat (replaceWith, lastLead)) + it.Substring (lastLead);

        }

        public static string ToCamelCase (string it, char delimSource, string delimDest, bool force = false) {
            if (string.IsNullOrEmpty (it) || (!force && !it.Contains (delimSource))) {
                return it;
            }
            var array = it.Split (delimSource);
            for (var i = 0; i < array.Length; i++) {
                var s = array[i];
                var first = string.Empty;
                if (s.Length > 0) {
                    first = Char.ToUpperInvariant (s[0]).ToString ();
                }
                var rest = string.Empty;
                if (s.Length > 1) {
                    rest = s.Substring (1).ToLowerInvariant ();
                }
                array[i] = first + rest;
            }
            var newname = string.Join (delimDest, array);
            if (newname.Length == 0) {
                newname = it;
            }
            return newname;
        }
        public static string UnderscoreToCamelCase (this string it, bool force = false) => ToCamelCase (it, '_', "", force);

        public static string BlankToCamelCase (this string it, bool force = false) => ToCamelCase (it, ' ', "", force);

        public static string GetResourceStream (Assembly assembly, string name) {

            var resources = assembly.GetManifestResourceNames ();
            var resourceName = resources.SingleOrDefault (str => str == name);

            // try harder:
            if (resourceName == default) {
                resourceName = resources.SingleOrDefault (str => str.EndsWith (name));
            }

            if (resourceName == default)
                return default;

            using (var stream = assembly.GetManifestResourceStream (resourceName))
            using (var reader = new StreamReader (stream)) {
                var result = reader.ReadToEnd ();
                return result;
            }
        }

        public static IEnumerable<(string name, string content)> GetResourceStreams(Assembly assembly, Func<string,bool> acceptname) {

            var resources = assembly.GetManifestResourceNames();
            var resourceNames = resources.Where(acceptname);

            foreach(var resourceName in resourceNames)
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream)) {
                var result = reader.ReadToEnd();
                yield return (resourceName, result);
            }
        }
        
        public static IEnumerable<string> LinesOf (this TextReader it) {

            var line = it.ReadLine ();
            while (line != null) {
                yield return line;
                line = it.ReadLine ();
            }
  
        }
    }
}
