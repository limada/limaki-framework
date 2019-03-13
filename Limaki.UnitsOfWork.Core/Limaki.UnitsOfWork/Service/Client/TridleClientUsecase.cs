﻿/*
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

using System;
using System.Linq;
using System.Reflection;
using Limaki.Common.Collections;
using Limaki.UnitsOfWork.Model;
using Limaki.UnitsOfWork.Usecases;

namespace Limaki.UnitsOfWork {

    public class TridleClientUsecase {

        public Store Store { get; set; }
        public Func<Store, ITridleQueryPredicates, ITridleContainer> LoadRequest { get; set; }
        public Action<Store> SaveChanges { get; set; }
        public Func<ITridleQueryPredicates> CreatePredicates { get; set; }

        GuidFlags _resolve = null;
        GuidFlags Resolve {
            get {
                if (_resolve == null) {
                    var preds = CreatePredicates ();
                    var type = preds.GetType ()
                        .GetProperties ()
                                    .Where (p => p.DeclaringType == preds.GetType () && p.Name == nameof (ITridleQueryPredicates.Resolve)).First ().PropertyType;
                    _resolve = (GuidFlags)Activator.CreateInstance (type);
                    _resolve.Add(
                        _resolve.FlagOf (nameof (Model.Dto.StringTridle)),
                        _resolve.FlagOf (nameof (Model.Dto.NumberTridle)),
                        _resolve.FlagOf (nameof (Model.Dto.RelationTridle))
                    );
                }
                return _resolve;
            }
        }

        public IStringTridle GetStringTridle (Store store, Guid key, Guid member) {
            var tridle = store.Item<IStringTridle> (t => t.Key == key && t.Member == member);
            if (tridle == null) {
                var preds = CreatePredicates ();
                preds.Resolve = Resolve;
                preds.StringTridles = t => t.Key == key && t.Member == member;
                LoadRequest (store, preds);
                tridle = store.Item<IStringTridle> (t => t.Key == key && t.Member == member);
                if (tridle == null) {
                    tridle = Store.AddCreated (store.Create<IStringTridle> ());
                    tridle.Key = key;
                    tridle.Member = member;
                }
            }
            return tridle;
        }

        public string GetStringTridleValue (Store store, Guid key, Guid member) => GetStringTridle (store, key, member)?.Value;

        public bool SetStringTridleValue (Store store, Guid key, Guid member, string value) {
            var tridle = GetStringTridle (store, key, member);
            var vm = new ViewModel<object> (null) { Store = store };
            return vm.EntityChanged (tridle, (t, s) => t.Value = s, tridle.Value, value, nameof (IStringTridle.Value));
        }

        public INumberTridle GetNumberTridle (Store store, Guid key, Guid member) {

            var tridle = store?.Item<INumberTridle> (t => t.Key == key && t.Member == member);
            if (tridle != null)
                return tridle;

            var preds = CreatePredicates ();
            preds.Resolve = Resolve;
            preds.NumberTridles = t => t.Key == key && t.Member == member;
            if (store == null) {
                store = new Store { ItemFactory = Store.ItemFactory };
                var cont = LoadRequest (store, preds);
                tridle = cont.NumberTridles?.FirstOrDefault ();
            } else {
                LoadRequest (store, preds);
                tridle = store.Item<INumberTridle> (t => t.Key == key && t.Member == member);
            }

            if (tridle == null) {
                tridle = store.AddCreated (store.Create<INumberTridle> ());
                tridle.Key = key;
                tridle.Member = member;
            }

            return tridle;
        }

        public long GetNumberTridleValue (Store store, Guid key, Guid member) => GetNumberTridle (store, key, member)?.Value ?? 0;

        public bool SetNumberTridleValue (Store store, Guid key, Guid member, long value) {
            bool save = false;
            var changed = false;
            if (store == null) {
                store = new Store { ItemFactory = Store.ItemFactory };
                save = true;
            }
            var tridle = GetNumberTridle (store, key, member);
            if (tridle.Value != value) {
                tridle.Value = value;
                store.Update (tridle);
                changed = true;
            }

            if (save || changed) {
                SaveChanges (store);
            }
            return changed;
        }
    }

}
