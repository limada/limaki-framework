/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2008-2012 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Xml.Linq;
using System.Diagnostics;

namespace Limaki.Common.Services {
    [ConfigurationSection(typeof(ServiceSettings))]
    public class ServiceSettings : ConfigurationSettings,IServiceSettings {

        [ConfigurationProperty("Port")]
        public int Port { get; set; }
        [ConfigurationProperty("Prefix")]
        public string Prefix { get; set; }
        [ConfigurationProperty("IP")]
        public string IP { get; set; }
        [ConfigurationProperty("Binding")]
        public BindingFlag Binding { get; set; }
        
        //not working: [ConfigurationProperty("Timeout",Optional=true)]
        public TimeSpan Timeout { get; set; }


        public ServiceSettings() {
            Timeout = new TimeSpan(0, 1, 0);
        }

        public static IServiceSettings Load(string sectionName) {
            var result = new ServiceSettings();
            var file = ConfigFile();
            if (file != null) {
                var appConfig = XDocument.Load(file);

                Visit(appConfig, sectionName, (key, value) => {
                    if (key.Value == "Port")
                        result.Port = int.Parse(value.Value);
                    if (key.Value == "Prefix")
                        result.Prefix = value.Value;
                    if (key.Value == "Binding")
                        result.Binding = (BindingFlag)Enum.Parse(typeof(BindingFlag), value.Value);
                    if (key.Value == "IP")
                        result.IP = value.Value;
                    if (key.Value == "Timeout") {
                        var to = default(TimeSpan);
                        if (TimeSpan.TryParse(value.Value, out to))
                            result.Timeout = to;
                    }
                });

                Trace.WriteLine(result.Info());
            }

            return result;
        }

        public string Info() {
            return string.Format("Service listening on: {0}:{1}/{2}", this.IP, this.Port, this.Prefix);
        }
    }
}