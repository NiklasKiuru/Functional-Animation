using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Vector2Interpolator : IVectorInterpolator<float, float2>
    {
        public int2 Stride;
        public int AxisCount { get => 3; }
        public void SetValue(int index, float value, ref float2 current)
        {
            current[index] = value;
        }
        public int PointerCount(int index) => Stride[index];
        public float InterpolateAxis(float2 from, float2 to, RangedFunction func, float time, int index)
        {
            var start = from[index];
            var end = to[index];
            return func.Interpolate(start, end, time);
        }
        public float2 Interpolate(float2 from, float2 to, RangedFunction func, float time)
        {
            return new float2()
            {
                x = func.Interpolate(from.x, to.x, time),
                y = func.Interpolate(from.y, to.y, time),
            };
        }
    }
}

