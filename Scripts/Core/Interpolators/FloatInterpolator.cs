using Unity.Burst;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct FloatInterpolator : IInterpolator<float>
    {
        public int Stride;
        bool IGroupProcessor.IsAlive { get; set; }
        int IGroupProcessor.Id { get; set; }
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public float Current { get; set; }
        public Clock Clock { get; set; }
        public float From { get; set; }
        public float To { get; set; }
        public int AxisCount { get => 1; }
        [BurstDiscard]
        public IInterpolator<float> ReRegister(FunctionContainer cont)
        {
            EFAnimator.RegisterTargetNonAlloc<float, FloatInterpolator>(ref this, cont);
            return this;
        }
        [BurstDiscard]
        public float GetRealTimeValue() => EFAnimator.GetValueExternal<float, FloatInterpolator>(this);
        public int GetGroupId() => sizeof(float);
        public void SetValue(int index, float value) => Current = value;
        public bool IsValid(int index) => true;
        public int PointerCount(int index) => Stride;
        public float ReadFrom(int index) => From;
        public float ReadTo(int index) => To;
    }
}

