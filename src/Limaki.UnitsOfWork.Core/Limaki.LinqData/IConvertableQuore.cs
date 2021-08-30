using System;
using System.Linq.Expressions;

namespace Limaki.LinqData {

    public interface IConvertableQuore : IQuore {
        Func<Expression, Type, Expression> Convert { get; set; }
    }

}