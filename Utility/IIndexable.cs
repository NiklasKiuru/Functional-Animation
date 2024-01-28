namespace Aikom.FunctionalAnimation.Utility
{
    public interface IIndexable<T, D> where D : System.Enum
    {
        public int Length { get; }
        public T this[D index] { get; }
    }
}

