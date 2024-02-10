
namespace Aikom.FunctionalAnimation
{
    public interface IInterpolator<T> : IInterpolatorHandle<T> where T : unmanaged
    {   
        
        public T IncrimentValue();
        public T GetValue();
    }
}
