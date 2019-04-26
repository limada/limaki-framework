namespace Limaki.Data {

    public interface IDomainQuoreFactory<T> where T : IDomainQuore {
        bool Supports (IDbProvider provider);
        IDbGateway CreateGateway (IDbProvider provider);
        IQuore CreateQuore (IDbGateway gateway);
        T CreateDomainQuore (Iori iori);
    }

}
