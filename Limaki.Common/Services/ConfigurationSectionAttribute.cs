using System;
using System.Linq;

namespace Limaki.Common {

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConfigurationSectionAttribute : Attribute {
        public ConfigurationSectionAttribute(string sectionName) {
            SectionName = sectionName;
        }

        public ConfigurationSectionAttribute(Type type) {
            SectionName = type.FullName;
        }

        /// <summary>
        /// name of the ConfigurationSection
        /// </summary>
        public string SectionName { get; protected set; }

        public bool Optional { get; set; }

        public static ConfigurationSectionAttribute GetAttribute (System.Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            return ((ConfigurationSectionAttribute[])
                   type.GetCustomAttributes(typeof(ConfigurationSectionAttribute), false))
                       .FirstOrDefault();
        }
    }
}