/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2018-2020 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Limaki.Common.IOC {

    public class AppSettings {

        public static Func<NameValueCollection> AppSettingsGetter { get; set; }
        public static Action<string, string> AppSettingsSetter { get; set; }

        public static string GetValueFromAppSettings (string key) {
            var appSettings = AppSettingsGetter?.Invoke () ?? ConfigurationManager.AppSettings;
            var value = appSettings.Get (key);
            return value;
        }

        public static Iori GetIoriFromAppSettings (string key) {
            if (GetValueFromAppSettings (key) is string value) {
                var iori = new Iori ().FromSettingsKey (value);
                return iori;
            }
            return default;
        }
    }
}