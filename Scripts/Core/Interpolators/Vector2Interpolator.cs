using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Vector2Interpolator : IInterpolator<float2>
    {
        public int2 Stride;
        public bool2 AxisCheck;

        private float2 _current;
        bool IGroupProcessor.IsAlive { get; set; }
        int IGroupProcessor.Id { get; set; }
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public float2 Current { get => _current; set => _current = value; }
        public Clock Clock { get; set; }
        public float2 From { get; set; }
        public float2 To { get; set; }
        public int AxisCount { get => 2; }
        [BurstDiscard]
        public IInterpolator<float2> ReRegister(FunctionContainer cont)
        {
            EFAnimator.RegisterTargetNonAlloc<float2, Vector2Interpolator>(ref this, cont);
            return this;
        }
        [BurstDiscard]
        public float2 GetRealTimeValue() => EFAnimator.GetValueExternal<float2, Vector2Interpolator>(this);
        public int GetGroupId() => sizeof(float) * 2;
        public void SetValue(int index, float value) => _current[index] = value;
        public bool IsValid(int index) => AxisCheck[index];
        public int PointerCount(int index) => Stride[index];
        public float ReadFrom(int index) => From[index];
        public float ReadTo(int index) => To[index];
    }
}

