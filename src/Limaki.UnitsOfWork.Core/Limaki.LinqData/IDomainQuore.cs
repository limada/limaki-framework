using System;

namespace Limaki.LinqData {

    /// <summary>
    /// base interface for domain specific quore
    /// </summary>
    public interface IDomainQuore : IDisposable {
        
        IQuore Quore { get; }

    }
    
}
