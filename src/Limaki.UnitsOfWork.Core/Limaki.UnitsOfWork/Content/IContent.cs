using System;

namespace Limaki.UnitsOfWork.Content {

    public interface IContent {

        Guid Compression { get; set; }

        Guid ContentType { get; set; }

        object Description { get; set; }

        object Source { get; set; }

    }

    public interface IContent<T> : IContent {

        T Data { get; set; }

    }

    public interface IContent<TKey, TData> : IContent<TData> {

        TKey Id { get; set; }

    }

}