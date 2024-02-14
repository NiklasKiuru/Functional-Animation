
using UnityEditor.Build;

namespace Aikom.FunctionalAnimation
{
    public interface IInterpolator<T> : IInterpolatorHandle<T> where T : unmanaged
    {   
        public EventFlags ActiveFlags { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public ExecutionStatus Status { get; set; }
        public int InternalId { get; set; }
        public int Length { get; }
        public T Current { get; }
    }
}
