using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Vector3Interpolator : IInterpolator<float3>
    {
        public int3 Stride;
        public bool3 AxisCheck;

        private float3 _current;
        bool IGroupProcessor.IsAlive { get; set; }
        int IGroupProcessor.Id { get; set; }
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public float3 Current { get => _current; set => _current = value; }
        public Clock Clock { get; set; }
        public float3 From { get; set; }
        public float3 To { get; set; }
        public int AxisCount { get => 3; }
        [BurstDiscard]
        public IInterpolator<float3> ReRegister(FunctionContainer cont)
        {
            EFAnimator.RegisterTargetNonAlloc<float3, Vector3Interpolator>(ref this, cont);
            return this;
        }
        [BurstDiscard]
        public float3 GetRealTimeValue() => EFAnimator.GetValueExternal<float3, Vector3Interpolator>(this);
        public int GetGroupId() => sizeof(float) * 3;
        public void SetValue(int index, float value) => _current[index] = value;
        public bool IsValid(int index) => AxisCheck[index];
        public int PointerCount(int index) => Stride[index];
        public float ReadFrom(int index) => From[index];
        public float ReadTo(int index) => To[index];

    }
}

