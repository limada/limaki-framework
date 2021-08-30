using Limaki.Data;

namespace Limaki.LinqData {

    public interface IDomainQuoreFactory<T> where T : IDomainQuore {
        bool Supports (IDbProvider provider);
        IDbGateway CreateGateway (IDbProvider provider);
        IQuore CreateQuore (IDbGateway gateway);
        T CreateDomainQuore (Iori iori);
    }

}
