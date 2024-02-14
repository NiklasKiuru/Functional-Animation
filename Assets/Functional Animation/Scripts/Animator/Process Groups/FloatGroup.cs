using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class FloatGroup : GroupBase<IInterpolator<float>, float, FloatInterpolator>
    {
        public FloatGroup(int preallocSize) : base(preallocSize)
        {
        }

        public override int GroupId { get => sizeof(float); }

        protected unsafe override bool ProcessInternal()
        {   
            UnsafeExecutionUtility.InterpolateFloats((RangedFunction*)_functions.GetUnsafeReadOnlyPtr(),
                    (FloatInterpolator*)_processors.GetUnsafePtr(), Time.deltaTime,
                    out var hasEvents, (FlagIndexer<float>*)_events.GetUnsafePtr(), _processors.Length);
            return hasEvents;
        }
    }
}

