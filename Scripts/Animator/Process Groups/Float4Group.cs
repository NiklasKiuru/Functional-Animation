using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float4Group : GroupBase<float4, Vector4Interpolator>
    {
        public Float4Group(int preallocSize) : base(preallocSize)
        {
        }

        public override int GroupId => sizeof(float) * 4;
        protected override int Dimension => 4;

        protected unsafe override bool ProcessInternal()
        {
            UnsafeExecutionUtility.Interpolate4Floats((RangedFunction*)_functions.GetUnsafeReadOnlyPtr(),
                    (Vector4Interpolator*)_processors.GetUnsafePtr(), Time.deltaTime,
                    out var hasEvents, (EventData<float4>*)_events.GetUnsafePtr(), _processors.Length);
            return hasEvents;
        }
    }
}

