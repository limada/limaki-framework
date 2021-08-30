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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;


namespace Limaki.Common {

    ///<summary>
    /// Input Output Resource Identifier
    /// provides information needed to connect to a database
    /// </summary>
    ///  <stereotype>description</stereotype>
    [DataContract]
    public class Iori {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Server { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string Extension { get; set; }

        [DataMember]
        public string Provider { get; set; }

        [DataMember]
        public IoMode AccessMode { get; set; }

        [DataMember]
        public string Optional { get; set; }

        /// <summary>
        /// extracts the information of filename and fills dataBaseInfo 
        /// with server, path, name and provider according to filename
        /// </summary>
        /// <param name="fileName"></param>
        public static Iori FromFileName (string fileName) {
            var iori = new Iori ();
            IoriExtensions.FromFileName (iori, fileName);
            return iori;
        }

        public override int GetHashCode () {
            int h = (Path?.GetHashCode () ?? 1);
            h = (h << 5) - h + (Name?.GetHashCode () ?? 1);
            h = (h << 5) - h + (Extension?.GetHashCode () ?? 1);
            h = (h << 5) - h + (Server?.GetHashCode () ?? 1);
            h = (h << 5) - h + Port.GetHashCode ();
            h = (h << 5) - h + (User?.GetHashCode () ?? 1);
            h = (h << 5) - h + (Password?.GetHashCode () ?? 1);
            h = (h << 5) - h + (Provider?.GetHashCode () ?? 1);
            h = (h << 5) - h + (Optional?.GetHashCode () ?? 1);
            h = (h << 5) - h + AccessMode.GetHashCode ();
            return h;
        }

        public override bool Equals (object obj) {
            if (base.Equals (obj))
                return true;
            if (obj is Iori other) {
                return other.Path == Path && other.Name == Name && other.Extension == Extension && other.Server == Server && other.Port == Port &&
                       other.User == User && other.Password == Password && other.Provider == Provider && other.AccessMode == AccessMode
                       && other.Optional == Optional;
            }
            return false;
        }

        public static Iori Clone (Iori other) => new Copier<Iori> ().Copy (other, new Iori ());

        public override string ToString () => $"{this.ToFileName ()} | {Provider}";
    }

    public static class IoriExtensions {

        public static string ToFileName (this Iori iori) {
            var extension = iori.Extension ?? "";
            if (!extension.StartsWith (".") && !string.IsNullOrEmpty (extension))
                extension = "." + extension;
            var sep = Path.DirectorySeparatorChar.ToString ();
            var path = iori.Path ?? "";
            if (!(path.EndsWith (sep) || string.IsNullOrEmpty (path)))
                path = path + sep;
            return path + iori.Name + extension;
        }

        public static void FromFileName (this Iori iori, string fileName) {
            var file = new FileInfo (fileName);
            iori.Server = "localhost";
            iori.Path = file.DirectoryName + Path.DirectorySeparatorChar;
            iori.Name = Path.GetFileNameWithoutExtension (file.FullName);
            iori.Extension = Path.GetExtension (file.FullName).ToLower ();
        }

        public static Iori FromXmlStream (this Iori iori, string rootElement, Stream source) {

            var reader = XmlReader.Create (source, new XmlReaderSettings () {
                ConformanceLevel = ConformanceLevel.Fragment
            });
            var serializer = new DataContractSerializer (typeof (Iori));

            while (!reader.EOF && !reader.IsStartElement (rootElement)) {
                reader.Skip ();
            }
            if (reader.IsStartElement (rootElement)) {
                reader.ReadStartElement ();
                iori.FromIori (serializer.ReadObject (reader) as Iori);
            }

            return iori;
        }

        public static Iori FromXmlFile (this Iori iori, string rootElement, string fileName) {
            using (var s = File.OpenRead (fileName)) {
                return FromXmlStream (iori, rootElement, s);
            }
        }

        public static void ToXmlStream (this Iori iori, string rootElement, Stream sink) {

            var writer = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sink, new XmlWriterSettings {
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                CloseOutput = false,
            }));
            var serializer = new DataContractSerializer (typeof (Iori));
            writer.WriteStartElement (rootElement);
            serializer.WriteObject (writer, iori);
            writer.WriteEndElement ();
            writer.Flush ();
        }

        const char appDelim = '|';
        public static Iori FromAppSettings (this Iori iori, NameValueCollection appSettings) {
            var settingsString = new StringBuilder ();
            for (int i = 0; i < appSettings.Count; i++) {
                string key = appSettings.GetKey (i).Replace ("Database", "");
                string data = appSettings.Get (i);

                settingsString.Append ($"{key}={data}{appDelim}");
            }
            return FromSettingsKey (iori, settingsString.ToString ());

        }

        public static Iori FromSettingsKey (this Iori iori, string appSettings) {
            if (appSettings == null)
                return iori;

            foreach (var value in appSettings.Split (appDelim)) {

                var val = value.Trim ();

                if (string.IsNullOrWhiteSpace (val))
                    continue;

                var setting = val.Split (new [] { '=' }, 2);
                string key = setting.FirstOrDefault ().Trim ();
                string data = setting.LastOrDefault ().Trim ();

                if (key == "FileName") {
                    FromFileName (iori, data);
                }

                if (key == nameof (Iori.Server)) {
                    iori.Server = data;
                }
                if (key == nameof (Iori.Name)) {
                    iori.Name = data;
                }
                if (key == nameof (Iori.Extension)) {
                    iori.Extension = data;
                }
                if (key == nameof (Iori.Path)) {
                    iori.Path = data;
                }
                if (key == nameof (Iori.User)) {
                    iori.User = data;
                }
                if (key == nameof (Iori.Password)) {
                    iori.Password = data;
                }
                if (key == nameof (Iori.Provider)) {
                    iori.Provider = data;
                }
                if (key == nameof (Iori.Port) && int.TryParse (data, out var port)) {
                    iori.Port = port;
                }
                if (key == nameof (Iori.Optional)) {
                    iori.Optional = data;
                }

                if (key == nameof(Iori.AccessMode)) {
                    if (Enum.TryParse<IoMode>(data, out var ioMode))
                        iori.AccessMode = ioMode;
                }

            }
            return iori;

        }

        static PropertyInfo[] _props = default;
        static PropertyInfo[] Props => _props ?? (_props = typeof(Iori).GetProperties().Where(m => m.GetCustomAttribute<DataMemberAttribute>() != null).ToArray());
        public static string AsSettingsKey(this Iori iori, bool hidePw = true)
        {
            if (iori == null)
                return null;
            return string.Join("|", Props.Select(p => new { p.Name, val = p.GetValue(iori) })
                .Where(v =>
                {
                    if (v.Name == nameof(Iori.Password) && hidePw)
                        return false;
                    if (v.Name == nameof(Iori.AccessMode) && iori.AccessMode==IoMode.None)
                        return false;
                    if (v.Name == nameof(Iori.Port) && iori.Port == default)
                        return false;
                    if (v.Name == nameof(Iori.Extension) && iori.Server != default)
                        return false;
                    return v.val != default;
                })
                .Select(v => $"{v.Name}={v.val}"));
        }

        public static Iori FromIori (this Iori iori, Iori other) => new Copier<Iori> ().Copy (other, iori);
    }
}