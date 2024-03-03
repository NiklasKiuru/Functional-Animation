namespace Aikom.FunctionalAnimation
{
    public interface IGroupProcessor
    {
        public int GetGroupId();
        /// <summary>
        /// Process id of this handle
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Alive status of the process
        /// </summary>
        public bool IsAlive { get; internal set; }

        /// <summary>
        /// Shortcut struct for process identifier
        /// </summary>
        /// <returns></returns>
        public ProcessId GetIdentifier()
        {
            return new ProcessId(Id, GetGroupId());
        }
    }

    public struct ProcessId
    {
        public int Id { get; }
        public int GroupId { get; }

        public ProcessId(int id, int group)
        {
            Id = id;
            GroupId = group;
        }
    }
}

