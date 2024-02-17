using Unity.Burst;

namespace Aikom.FunctionalAnimation
{
    public interface IInterpolator<T> : IInterpolatorHandle<T> where T : unmanaged
    {   
        public EventFlags ActiveFlags { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public ExecutionStatus Status { get; set; }
        public int InternalId { get; set; }
        public T Current { get; set; }
        public T From { get; }
        public T To { get; }
        public int AxisCount { get; }
        public Clock Clock { get; set; }
        public void SetValue(int index, float value);
        public bool IsValid(int index);
        public int PointerCount(int index);
        public float ReadFrom(int index);
        public float ReadTo(int index);
        public IInterpolatorHandle<T> Register(FunctionContainer cont);
    }

    public static class InterpExtensions
    {   
        /// <summary>
        /// Restarts the internal clock of the interpolator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static void Restart<T>(this IInterpolator<T> handle) where T : unmanaged
        {
            var clock = handle.Clock;
            clock.Reset();
            handle.Clock = clock;
        }
    }
}
