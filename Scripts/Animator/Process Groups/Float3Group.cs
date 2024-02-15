using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class Float3Group : GroupBase<IInterpolator<float3>, float3, Vector3Interpolator>
    {
        public Float3Group(int preallocSize) : base(preallocSize)
        {
        }

        public override int GroupId => sizeof(float) * 3;

        protected unsafe override bool ProcessInternal()
        {
            UnsafeExecutionUtility.Interpolate3Floats((RangedFunction*)_functions.GetUnsafeReadOnlyPtr(),
                    (Vector3Interpolator*)_processors.GetUnsafePtr(), Time.deltaTime,
                    out var hasEvents, (FlagIndexer<float3>*)_events.GetUnsafePtr(), _processors.Length);
            return hasEvents;
        }
    }
}