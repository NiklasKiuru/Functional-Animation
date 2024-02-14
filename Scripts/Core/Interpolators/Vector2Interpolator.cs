using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public struct Vector2Interpolator : IInterpolator<float2>
    {
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public int InternalId { get; set; }
        bool IInterpolatorHandle<float2>.IsAlive { get; set; }
        public int Length { get => math.csum(Stride); }

        public float2 From;
        public float2 To;
        public float2 Current { get; internal set; }
        public int2 Stride;
        public bool2 AxisCheck;
        public Clock Clock;

        public int GetGroupId() => sizeof(float) * 2;
    }
}

