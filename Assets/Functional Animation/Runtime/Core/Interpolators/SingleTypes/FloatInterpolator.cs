using Unity.Burst;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct FloatInterpolator : IInterpolator<float>
    {
        public float Interpolate(float from, float to, RangedFunction func, float time)
        {
            return func.Interpolate(from, to, time);
        }
    }
}

