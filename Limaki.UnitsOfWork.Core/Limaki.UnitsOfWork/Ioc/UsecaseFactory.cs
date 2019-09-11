using Limaki.Common;

namespace Limaki.Usecases {

    public class UsecaseFactory<T> where T : new() {

        public virtual T Create () => new T ();

        public IComposer<T> Composer { get; set; }
        public IComposer<T> BackendComposer { get; set; }

        public virtual void Compose (T useCase) {
            Composer.Factor (useCase);
            BackendComposer.Factor (useCase);

            BackendComposer.Compose (useCase);
            Composer.Compose (useCase);

        }
    }
}