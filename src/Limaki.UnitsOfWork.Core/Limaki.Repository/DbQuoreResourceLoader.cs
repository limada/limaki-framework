using Limaki.Common.IOC;
using Limaki.Data;

namespace Limaki.Repository {

    public class DbQuoreResourceLoader : IContextResourceLoader {

        public void ApplyResources (IApplicationContext context) {

            var dbProviderPool = context.Pooled<DbProviderPool> ();
            dbProviderPool.Add (new InMemoryDbQuoreProvider ());

        }
    }
}
