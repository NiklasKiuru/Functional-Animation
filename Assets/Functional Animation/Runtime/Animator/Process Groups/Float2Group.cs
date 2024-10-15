using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float2Group : VectorGroup<float2, Vector2Interpolator, float>
    {
        public Float2Group(int preallocSize) : base(preallocSize)
        {
        }

    }
}

