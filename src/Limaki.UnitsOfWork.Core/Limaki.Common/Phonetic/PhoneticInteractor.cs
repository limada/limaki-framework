/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2009-2019 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Limaki.UnitsOfWork;
using PhoneticNet;

namespace Limaki.Common.Phonetics {

    public class PhoneticInteractor {

        public bool UseStopwords { get; set; }
        ICollection<string> _stopwords = null;

        public virtual ICollection<string> Stopwords => _stopwords ??= new HashSet<string> {
            // split-words:
            "van",
            "de",
            "da",
            "la",
            "sir",
            "von", // v.
        };

        Regex _regex = null;
        public virtual Regex Regex => _regex ??= new Regex (@"\W+", RegexOptions.Compiled);

        public IEnumerable<string> Words (string words) {
            if (string.IsNullOrEmpty (words))
                yield break;

            foreach (var w in Regex.Split (words).OrderBy (e => e)) {
                if (!string.IsNullOrEmpty (w)) {
                    if (!UseStopwords)
                        yield return w;
                    else if (!Stopwords.Contains (w))
                        yield return w;
                }
            }
        }

        Guid _algoId;
        IPhonetic _phonetic;

        public IPhonetic Phonetic {
            get {
                if (_phonetic == default) {
                    _algoId = DoubleMetaphone;
                    _phonetic = new DoubleMetaphone ();
                }

                return _phonetic;
            }
        }

        public Guid PhoneticId {
            get => _algoId == default ? (_algoId = SetAlgo (DoubleMetaphone).id) : _algoId;
            set => SetAlgo (value);
        }

        public static Guid GetProvider (string algo) {
            var prop = typeof(PhoneticInteractor)
                .GetProperties (BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .FirstOrDefault (p => p.PropertyType == typeof(Guid) && p.Name == algo);

            if (prop?.GetValue (null) is Guid result)
                return result;
            return default;
        }

        static IEnumerable<(Type Type, Guid guid)> _phoneticTypes = default;

        IEnumerable<(Type Type, Guid guid)> PhoneticTypes =>
            _phoneticTypes ??= GetType ().GetProperties (BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                .Where (p => p.PropertyType == typeof(Guid) && p.IsDefined (typeof(TypeGuidAttribute)))
                .SelectMany (p => {
                    var guid = (Guid) p.GetValue (null);
                    return p.GetCustomAttributes<TypeGuidAttribute> ().Select (tg => (tg.Type, guid));
                });

        public IEnumerable<Type> Phonetics => PhoneticTypes.Select (t => t.Type);

        protected (IPhonetic phonetic, Guid id) SetAlgo (Guid algo) {
            if (algo == default || _algoId == algo)
                return (_phonetic, _algoId);

            var g = PhoneticTypes.FirstOrDefault (p => p.guid == algo);
            if (g != default && g.Type != default) {
                _algoId = g.guid;
                _phonetic = Activator.CreateInstance (g.Type) as IPhonetic;
            }

            return (_phonetic, _algoId);
        }

        public int KeyLength { get; set; }

        [TypeGuid (typeof(DaitchMokotoff))]
        public static Guid DaitchMokotoff { get; private set; } = new Guid ("1d8360e3-73ad-4732-92f8-51c84a17f47c");

        [TypeGuid (typeof(DaitchMokotoffDiacrits))]
        public static Guid DaitchMokotoffDiacrits { get; private set; } = new Guid ("f5821c41-e1a4-4f1f-bddf-b9ccb9a3e86c");

        [TypeGuid (typeof(DoubleMetaphone))]
        public static Guid DoubleMetaphone { get; private set; } = new Guid ("c1c3aa20-866f-4452-ae40-34c37e4d64d8");

        [TypeGuid (typeof(DoubleMetaphoneDiacrits))]
        public static Guid DoubleMetaphoneDiacrits { get; private set; } = new Guid ("840b0cbf-ae21-42a0-96ce-6d91db0cb4e5");

        [TypeGuid (typeof(Metaphone))]
        public static Guid Metaphone { get; private set; } = new Guid ("c13004b6-469d-4343-a595-8e494c317148");

        [TypeGuid (typeof(Soundex))]
        public static Guid Soundex { get; private set; } = new Guid ("afb72fe4-ca92-431a-ae6a-e4c7665a16dc");

        [TypeGuid (typeof(SoundexDiacrits))]
        public static Guid SoundexDiacrits { get; private set; } = new Guid ("00912978-b92b-45de-845e-af8f05af6825");

        [TypeGuid (typeof(Cologne))]
        public static Guid Cologne { get; private set; } = new Guid ("d5d6a905-83dc-4d3b-b255-0c1968db0ea1");

        [TypeGuid (typeof(CologneDiacrits))]
        public static Guid CologneDiacrits { get; private set; } = new Guid ("06c51838-c1ec-44a2-892d-5ea96ff32149");

        public virtual string PhoneticOf (string words) {
            return string.Join ("|", Words (words).Select (PhoneticOfWord));
        }

        public virtual string PhoneticOfWord (string word) {
            if (string.IsNullOrEmpty (word))
                return word;

            if (KeyLength != default)
                return Phonetic.GenerateKey (word, KeyLength);
            return Phonetic.GenerateKey (word);
        }

    }

}