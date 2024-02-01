namespace Aikom.FunctionalAnimation.Utility
{
    public interface ICustomIndexable<T, D> : IIndexable<T> where T : class
    {
        public T this[D index] { get => this[GetIndexer(index)]; }

        public int GetIndexer(D index);
    }

    public static class CustomIndexableExtensions
    {
        public static T Get<T, D>(this ICustomIndexable<T, D> indexable, D index) where T : class
        {   
            return indexable[indexable.GetIndexer(index)];
        }
    }
}

