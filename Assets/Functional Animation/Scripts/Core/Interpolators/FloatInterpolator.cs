
namespace Aikom.FunctionalAnimation
{
    public struct FloatInterpolator : IInterpolator<float>
    {
        /// <summary>
        /// Length of the function pointer array
        /// </summary>
        public int Length { get; internal set; }

        /// <summary>
        /// Interpolation start value
        /// </summary>
        public float From;

        /// <summary>
        /// Interpolation end value
        /// </summary>
        public float To;

        /// <summary>
        /// Internal clock
        /// </summary>
        public Clock Clock;

        public float Current { get; internal set; }
        public ExecutionStatus Status { get; set; }
        public EventFlags PassiveFlags { get; set; }
        public EventFlags ActiveFlags { get; set; }
        public int InternalId { get; set; }
        bool IInterpolatorHandle<float>.IsAlive { get; set; }

        public int GetGroupId() => sizeof(float);
    }
}

