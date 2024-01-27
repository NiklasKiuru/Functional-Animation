namespace Aikom.FunctionalAnimation.Utility
{
    public interface IIndexable<T>
    {
        public int Length { get; }
        public T this[int index] { get; }
    }
}

