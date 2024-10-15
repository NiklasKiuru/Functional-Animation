using System;

namespace Aikom.FunctionalAnimation
{
    public interface IInterpolator<T> where T : unmanaged
    {   
        public T Interpolate(T from, T to, RangedFunction func, float time);
    }

    public static class InterpExtensions
    {   
            
    }
}
