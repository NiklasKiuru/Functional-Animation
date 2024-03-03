using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float3Group : GroupBase<float3, Vector3Interpolator>
    {
        public Float3Group(int preallocSize) : base(preallocSize)
        {
        }

        public override int GroupId => sizeof(float) * 3;
        protected override int Dimension => 3;
    }
}
