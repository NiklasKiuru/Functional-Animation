using System;
using Unity.Collections;
using Unity.Jobs;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Interface for process groups
    /// </summary>
    /// <typeparam name="TProcessor"></typeparam>
    public interface IProcessGroup<TStruct, TProcessor> : IProcessGroupHandle 
        where TProcessor : unmanaged, IInterpolator<TStruct> 
        where TStruct : unmanaged
    {
    }

    /// <summary>
    /// Handle for process groups
    /// </summary>
    public interface IProcessGroupHandle : IDisposable
    {
        public int GroupId { get; internal set; }
        public int ProcAllocSize { get; }
        public JobHandle Process(PluginValueCache cache, ContextQueryResults query, JobHandle dep);
    }
}

