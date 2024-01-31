namespace Aikom.FunctionalAnimation.Utility
{
    public interface ICustomIndexable<T, D> : IIndexable<T> where T : class
    {
        public T this[D index] { get => this[GetIndexer(index)]; }

        public int GetIndexer(D index);
    }
}

