using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Vector4Interpolator : IVectorInterpolator<float, float4>
    {
        public int4 Stride;
        public int AxisCount { get => 3; }
        public void SetValue(int index, float value, ref float4 current)
        {
            current[index] = value;
        }
        public int PointerCount(int index) => Stride[index];
        public float InterpolateAxis(float4 from, float4 to, RangedFunction func, float time, int index)
        {
            var start = from[index];
            var end = to[index];
            return func.Interpolate(start, end, time);
        }
        public float4 Interpolate(float4 from, float4 to, RangedFunction func, float time)
        {
            return new float4()
            {
                x = func.Interpolate(from.x, to.x, time),
                y = func.Interpolate(from.y, to.y, time),
                z = func.Interpolate(from.z, to.z, time),
                w = func.Interpolate(from.w, to.w, time),
            };
        }
    }
}

