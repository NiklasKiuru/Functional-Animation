namespace Aikom.FunctionalAnimation
{
    public enum ExecutionStatus
    {   
        /// <summary>
        /// Default state
        /// </summary>
        Inactive,

        /// <summary>
        /// Active state
        /// </summary>
        Running,

        /// <summary>
        /// Paused. Will not get auto removed
        /// </summary>
        Paused,

        /// <summary>
        /// Will get autoremoved
        /// </summary>
        Completed,
    }
}
