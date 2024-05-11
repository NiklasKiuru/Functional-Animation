namespace Aikom.FunctionalAnimation
{
    public interface IInterpolator<T> : IGroupProcessor where T : unmanaged
    {   
        public EventFlags ActiveFlags { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public ExecutionStatus Status { get; set; }
        public T Current { get; set; }
        public T From { get; set; }
        public T To { get; set; }
        public int AxisCount { get; }
        public Clock Clock { get; set; }
        public void SetValue(int index, float value);
        public bool IsValid(int index);
        public int PointerCount(int index);
        public float ReadFrom(int index);
        public float ReadTo(int index);
        public IInterpolator<T> ReRegister(FunctionContainer cont);
        public T GetRealTimeValue();
    }

    public static class InterpExtensions
    {   
        /// <summary>
        /// Restarts the internal clock of the interpolator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolator<T> Restart<T>(this IInterpolator<T> handle) where T : unmanaged
        {
            var clock = handle.Clock;
            clock.Reset();
            handle.Clock = clock;
            return handle;
        }

        /// <summary>
        /// Sets max loop count of the process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="count"></param>
        public static IInterpolator<T> SetMaxLoopCount<T>(this IInterpolator<T> handle, int count)
            where T : unmanaged
        {
            var clock = handle.Clock;
            clock.MaxLoops = count;
            handle.Clock = clock;
            return handle;
        }

        /// <summary>
        /// Extended acces for IGroupProcessor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static ProcessId GetIdentifier<T>(this IInterpolator<T> handle) where T : unmanaged
        {
            return handle.GetIdentifier();
        }

        /// <summary>
        /// Flips the from and to values of the interpolator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IInterpolator<T> FlipValues<T>(this IInterpolator<T> handle) where T : unmanaged
        {
            var from = handle.From;
            var to = handle.To;
            handle.From = to;
            handle.To = from;
            return handle;
        }
    }
}
