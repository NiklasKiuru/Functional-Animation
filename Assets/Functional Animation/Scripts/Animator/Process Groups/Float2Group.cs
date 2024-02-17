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

        protected unsafe override bool ProcessInternal()
        {
            UnsafeExecutionUtility.Interpolate2Floats((RangedFunction*)_functions.GetUnsafeReadOnlyPtr(),
                    (Vector2Interpolator*)_processors.GetUnsafePtr(), Time.deltaTime,
                    out var hasEvents, (EventData<float2>*)_events.GetUnsafePtr(), _processors.Length);
            return hasEvents;
        }
    }
}

