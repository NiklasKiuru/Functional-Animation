using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float4Group : VectorGroup<float4, Vector4Interpolator, float>
    {
        public Float4Group(int preallocSize) : base(preallocSize)
        {
        }
    }
}

