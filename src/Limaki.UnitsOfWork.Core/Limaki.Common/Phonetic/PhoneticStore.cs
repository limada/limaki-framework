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
using Limaki.Common.Collections;

namespace Limaki.Common.Phonetics {

    public class PhoneticStore<T>
    {
        protected IMultiDictionary<string, T> PhoneticKeys = new MultiDictionary<string, T>();

        public Func<T, string> TextGetter { get; set; }
        public Func<string, IEnumerable<T>> ItemSearch { get; set; }

        public void Add(T item)
        {
            var text = TextGetter(item);
            if (text == default)
                return;
            
            var s = text.ToLower();
            var key = GenerateKey(s);

            PhoneticKeys.Add(key, item);

        }

        public void Add(T item, string text)
        {
            if (text == default)
                return;
            var s = text.ToLower();
            var key = GenerateKey(s);

            PhoneticKeys.Add(key, item);

        }

        PhoneticInteractor _phoneticInteractor = default;
        public PhoneticInteractor PhoneticInteractor => _phoneticInteractor ?? (_phoneticInteractor = new PhoneticInteractor());

        private string GenerateKey(string s) => PhoneticInteractor.PhoneticOf(s);

        public int PhonemCount => PhoneticKeys.Values.Count;

        public ICollection<T> Items(T item)
        {
            var key = GenerateKey(TextGetter(item).ToLower());
            if (PhoneticKeys.TryGetValue(key, out ICollection<T> result)) {
                return result;
            } else {
                return new T[0];
            }
        }

        public ICollection<T> ItemsFor(string words)
        {
            var key = GenerateKey(words.ToLower());
            if (PhoneticKeys.TryGetValue(key, out ICollection<T> result)) {
                return result;
            } else {
                return new T[0];
            }
        }
    }

}



