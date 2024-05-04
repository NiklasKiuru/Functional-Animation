using System;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Generic implimentation for ProcessGroupHandles. Allows adding members explicitly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProcessGroup<D> : IProcessGroupHandle<IGroupProcessor>
    {
        public int GroupId { get; }
        public void Add(D val, FunctionContainer cont);
        public void AddNonAlloc(D val, Span<RangedFunction> funcs);
        public D GetValue(int id);
    }

    /// <summary>
    /// Handle for process groups. This allows storing process groups by inheriting from this
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProcessGroupHandle<in T> : IDisposable where T : IGroupProcessor
    {
        public void Process();
        public void SetPassiveFlags(int id, EventFlags flags);
        public void ForceExecutionStatus(int id, ExecutionStatus status);
        public void ForceRemove(int id);
        public void PrecompileJobAssemblies();
        public void RestartProcess(int id);
        public void SetMaxLoopCount(int id, int count);
    }
}

