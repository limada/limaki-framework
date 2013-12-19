using System;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace Limaki.Common {

    public class ConfigurationSettings {

        public static string ConfigFile() {
            var dir = System.AppDomain.CurrentDomain.BaseDirectory;
            var file = Enumerable.FirstOrDefault<string>(Directory.GetFiles(dir, "*.config"));
            return file;
        }

        public static void Visit(XDocument appConfig, string sectionName, Action<XAttribute, XAttribute> visit) {
            var serviceSettings = appConfig.Root.Elements(sectionName).FirstOrDefault();

            if (serviceSettings != null) {
                foreach (var item in serviceSettings.Elements()) {
                    var key = item.Attribute("key");
                    var value = item.Attribute("value");
                    visit(key, value);
                }
            }
        }
    }
}