
public interface IIndexable<T> where T : class
{
    public int Length { get; }
    public T this[int index] { get; }
    
}
