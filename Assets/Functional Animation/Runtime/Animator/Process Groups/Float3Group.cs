using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float3Group : VectorGroup<float3, Vector3Interpolator, float>
    {
        public Float3Group(int preallocSize) : base(preallocSize)
        {
        }
    }
}
