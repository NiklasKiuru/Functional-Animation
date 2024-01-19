namespace Aikom.FunctionalAnimation
{
    public interface IInterpolateable<T> where T : struct
    {
        public T ReConstruct<D>(float[] values, T origin) where D : IInterpolateable<T>;
        public float[] Deconstruct();
    }
}

