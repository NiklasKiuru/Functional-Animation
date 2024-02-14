using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public struct Vector4Interpolator : IInterpolator<float4>
    {
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public int InternalId { get; set; }
        bool IInterpolatorHandle<float4>.IsAlive { get; set; }
        public int Length { get => math.csum(Stride); }

        public float4 From;
        public float4 To;
        public float4 Current { get; internal set; }
        public int4 Stride;
        public bool4 AxisCheck;
        public Clock Clock;

        public int GetGroupId() => sizeof(float) * 4;
    }
}

