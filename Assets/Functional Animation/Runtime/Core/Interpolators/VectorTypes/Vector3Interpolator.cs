using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Vector3Interpolator : IVectorInterpolator<float, float3>
    {
        public Vector3Interpolator(int3 stride)
        {
            Stride = stride;
        }

        public Vector3Interpolator(int count)
        {
            Stride = new int3(count);
        }

        public int3 Stride;
        public int AxisCount { get => 3; }
        public void SetValue(int index, float value, ref float3 current) 
        {
            current[index] = value;
        } 
        public int PointerCount(int index) => Stride[index];
        public float InterpolateAxis(float3 from, float3 to, RangedFunction func, float time, int index)
        {
            var start = from[index];
            var end = to[index];
            return func.Interpolate(start, end, time);
        }
        public float3 Interpolate(float3 from, float3 to, RangedFunction func, float time)
        {
            return new float3()
            {
                x = func.Interpolate(from.x, to.x, time),
                y = func.Interpolate(from.y, to.y, time),
                z = func.Interpolate(from.z, to.z, time),
            };
        }
    }
}

