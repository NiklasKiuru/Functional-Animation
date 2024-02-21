using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class FloatGroup : GroupBase<float, FloatInterpolator>
    {
        public FloatGroup(int preallocSize) : base(preallocSize)
        {
        }

        public override int GroupId { get => sizeof(float); }
        protected override int Dimension => 1;
    }
}

