using Limaki.Common.IOC;
using Limaki.Data;

namespace Limaki.LinqData {

    public class DbQuoreResourceLoader : IContextResourceLoader {

        public void ApplyResources (IApplicationContext context) {

            var dbProviderPool = context.Pooled<DbProviderPool> ();
            dbProviderPool.Add (new InMemoryDbQuoreProvider ());

        }
    }
}
