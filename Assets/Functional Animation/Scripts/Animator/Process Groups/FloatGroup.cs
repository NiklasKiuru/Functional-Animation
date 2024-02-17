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
        protected unsafe override bool ProcessInternal()
        {   
            UnsafeExecutionUtility.Interpolate((RangedFunction*)_functions.GetUnsafeReadOnlyPtr(),
                    (FloatInterpolator*)_processors.GetUnsafePtr(), Time.deltaTime,
                    out var hasEvents, (EventData<float>*)_events.GetUnsafePtr(), _processors.Length, 1);
            return hasEvents;
        }
    }
}

