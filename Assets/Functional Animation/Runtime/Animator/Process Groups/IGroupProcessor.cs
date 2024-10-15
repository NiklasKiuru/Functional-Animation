using System;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Aikom.FunctionalAnimation
{
    public interface IGroupProcessor
    {
        public EventFlags PassiveFlags { get; set; }
        public ExecutionStatus Status { get; set; }

        public int GetGroupId();
        /// <summary>
        /// Process id of this handle
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Alive status of the process
        /// </summary>
        [BurstDiscard]
        public bool IsAlive { get; internal set; }

        /// <summary>
        /// Shortcut struct for process identifier
        /// </summary>
        /// <returns></returns>
        public Process GetIdentifier()
        {
            return new Process(Id, GetGroupId());
        }

        public static int GetStaticGroupID<T, D>()
            where T : unmanaged
            where D : IInterpolator<T>
        {
            return typeof(D).GetHashCode()^typeof(T).GetHashCode();
        }
    }

    public readonly struct Process : IEquatable<Process>
    {
        public readonly int Id { get; }
        public readonly int GroupId { get; }
        public readonly int Version { get; }

        public Process(int id, int group)
        {
            Id = id;
            GroupId = group;
            Version = 0;
        }

        private Process(Process old)
        {
            Id = old.Id;
            GroupId = old.GroupId;
            Version = old.Version + 1;
        }

        internal Process IncrimentVersion()
        {
            return new Process(this);
        }

        public bool Equals(Process other)
        {
            return Id == other.Id && GroupId == other.GroupId && Version == other.Version;
        }
    }
}

