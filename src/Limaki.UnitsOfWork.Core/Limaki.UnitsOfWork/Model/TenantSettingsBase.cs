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
using Limaki.UnitsOfWork;
using System.IO;
using System.Collections.Specialized;
using Limaki.UnitsOfWork.Reporting;

namespace Limaki.UnitsOfWork.Model {
    
    public abstract class TenantSettingsBase : IReportSettingsProvider {
        
        public virtual Store Store { get; set; }

        public const string UserIdName = "lastuser";
        public Guid UserId { get; set; }

        public const string TenantIdName = "lasttenant";
        public Guid TenantId { get; set; }

        protected string _defaultDir => Path.GetDirectoryName (System.Reflection.Assembly.GetExecutingAssembly ().Location);

        public string TemplateDir { get; set; }

        public string OutputDir { get; set; }

        public Guid ReadGuidSettings (string name) {
            var value = AppSettings?.Get (name);
            if (value != null) {
                if (Guid.TryParse (value, out var guid)) {
                    return guid;
                }
            }
            return Guid.Empty;
        }

        public void WriteGuidSettings (Guid value, string name) {
            AppSettingsSetter?.Invoke (name, value.ToString ());
        }

        public NameValueCollection AppSettings { get; set; }
        public Action<string, string> AppSettingsSetter { get; set; }

        public abstract void ApplySettings ();
        public abstract void SaveAppSettings ();

        public static string NrFromPattern (string pattern, int nr) {
            try {
                return string.Format (pattern, nr);
            } catch (Exception ex) {
                return nr.ToString ();
            }
        }
    }
}