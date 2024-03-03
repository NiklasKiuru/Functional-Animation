using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float2Group : GroupBase<float2, Vector2Interpolator>
    {
        public Float2Group(int preallocSize) : base(preallocSize)
        {
        }

        public override int GroupId => sizeof(float) * 2;
        protected override int Dimension => 2;
    }
}

