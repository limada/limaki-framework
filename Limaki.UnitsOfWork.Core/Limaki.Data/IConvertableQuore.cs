using System;
using System.Linq.Expressions;

namespace Limaki.Data {

    public interface IConvertableQuore : IQuore {
        Func<Expression, Type, Expression> Convert { get; set; }
    }

}