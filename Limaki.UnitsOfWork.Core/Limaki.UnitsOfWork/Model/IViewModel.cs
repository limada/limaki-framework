namespace Limaki.UnitsOfWork {

    public interface IViewModel : ICheckable {

        Store Store { get; set; }
        bool ReadOnly { get; set; }
        string ToString (string format);

    }

    public interface IViewModel<T> : IViewModel {
        T Entity { get; }
    }
}