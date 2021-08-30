/*
 * Limaki 
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
using Limaki.Common;

namespace Limaki.Data {
    
    public static class IoriDataExtensions {
        public static string ConnectionString (this Iori iori) {

            var provider = iori.Provider.ToLower ();

            if (provider == "mysql") {
                return $"Server = {iori.Server}; Database = {iori.Name}; Uid = {iori.User}; Pwd = {iori.Password}; charset = utf8; pooling=false; SslMode=none";
            }

            if (provider == "firebird") {
                return $"User={iori.User};Password={iori.Password};Database={iori.ToFileName ()};DataSource={iori.Server};";
            }

            if (provider.StartsWith ("sqlite")) {
                return $"Data Source = {iori.ToFileName ()}";
            }

            if (provider.StartsWith ("postgres")) {
                var port = iori.Port == 0 ? "" : $"Port={iori.Port};";
                return $"User ID={iori.User};Password={iori.Password};Database={iori.ToFileName ()};Host={iori.Server};{port}";
            }

            if (provider.StartsWith ("sqlserver")) {
                var db = $"Initial Catalog={iori.Name};Data Source={iori.Server}";
                if (!String.IsNullOrEmpty (iori.Optional)) {
                    db = $"{db};{iori.Optional}";
                }
                db = db.EndsWith(";") ? db : $"{db};";
                if (iori.User?.ToLower () == "integrated security")
                    return $"{db}Integrated Security=True";

                return $"{db}User Id={iori.User};Password={iori.Password};";
            }

            if (provider.StartsWith ("oracle")) {
                return $"Data Source = {iori.Server}; User ID = {iori.User}; Password = {iori.Password};";
            }

            return null;

        }

        public static Iori Adjust (this Iori iori) {
            if (iori?.Provider == null)
                return iori;

            if (iori.Provider.ToLower().Contains("sqlite")) {
                if (!iori.Provider.ToLower().Contains("sqlite.")) {
                    iori = Iori.Clone(iori);
                    iori.Provider = Environment.OSVersion.VersionString.Contains("Unix") ?
                        iori.Provider.Replace("SQLite", "SQLite.Mono") :
                        iori.Provider.Replace("SQLite", "SQLite.Classic");
                }

                if (iori.Provider.EndsWith(".")) {
                    iori = Iori.Clone(iori);
                    iori.Provider = iori.Provider.Trim('.');
                }
            }

            return iori;
        }
    }
}