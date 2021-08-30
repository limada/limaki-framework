using System;
using System.Linq;
using System.Reflection;
using Limaki.Common;

namespace Limaki.Usecases {

    public class About {

        string _version = null;
        public string Version => _version ?? (_version = Assembly.GetName ().Version.ToString (4));

        string _company = null;
        public string Company => _company ?? (_company = AssemblyAttibue<AssemblyCompanyAttribute> ().Company);

        string _copyright = null;
        public string Copyright => _copyright ?? (_copyright = AssemblyAttibue<AssemblyCopyrightAttribute> ().Copyright);

        public virtual string Credits => 
            "Storage: db4o object database http://www.db4o.com \r\n"
            + "Graphics abstraction layer: http://github.com/mono/xwt \r\n"
            + "Icons: http://fortawesome.github.com/Font-Awesome \r\n";

        public string ToolKitType { get; set; }

        public Iori Iori { get; set; }

        public string ApplicationName { get; set; }

        public override string ToString () {
            return $@"
Version: {Version}

{Company} 
{Copyright}

ToolKitType: {ToolKitType ?? ""}

Database: {Iori?.ToString () ?? ""}

Credits: 

{Credits}
";
        }

        public virtual string Link => "www.limada.org";

        static Assembly Assembly => Assembly.GetAssembly (typeof (About));

        static A AssemblyAttibue<A> () where A : Attribute => Assembly.GetCustomAttributes (typeof (A), true).Cast<A> ().FirstOrDefault ();
    }
}