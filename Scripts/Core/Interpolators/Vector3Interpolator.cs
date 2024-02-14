using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public struct Vector3Interpolator : IInterpolator<float3>
    {
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public int InternalId { get; set; }
        bool IInterpolatorHandle<float3>.IsAlive { get; set; }
        public int Length { get => math.csum(Stride); }

        public float3 From;
        public float3 To;
        public float3 Current { get; internal set; }
        public int3 Stride;
        public bool3 AxisCheck;
        public Clock Clock;

        public int GetGroupId() => sizeof(float) * 3;
    }
}

