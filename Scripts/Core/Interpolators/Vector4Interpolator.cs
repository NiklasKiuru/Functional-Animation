using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Vector4Interpolator : IInterpolator<float4>
    {
        public int4 Stride;
        public bool4 AxisCheck;

        private float4 _current;
        bool IInterpolatorHandle<float4>.IsAlive { get; set; }
        int IInterpolatorHandle<float4>.Id { get => InternalId; set => InternalId = value; }
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public int InternalId { get; set; }
        public float4 Current { get => _current; set => _current = value; }
        public Clock Clock { get; set; }
        public float4 From { get; set; }
        public float4 To { get; set; }
        public int AxisCount { get => 4; }
        [BurstDiscard]
        public IInterpolatorHandle<float4> Register(FunctionContainer cont)
        {
            EFAnimator.RegisterTargetNonAlloc<float4, Vector4Interpolator>(ref this, cont);
            return this;
        }
        [BurstDiscard]
        public float4 GetValue() => EFAnimator.GetValueExternal<float4, Vector4Interpolator>(this);
        public int GetGroupId() => sizeof(float) * 4;
        public void SetValue(int index, float value) => _current[index] = value;
        public bool IsValid(int index) => AxisCheck[index];
        public int PointerCount(int index) => Stride[index];
        public float ReadFrom(int index) => From[index];
        public float ReadTo(int index) => To[index];
    }
}

