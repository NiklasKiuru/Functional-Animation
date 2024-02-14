using Aikom.FunctionalAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Generic implimentation for ProcessGroupHandles. Allows adding members explicitly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProcessGroup<T> : IProcessGroupHandle<IGroupProcessor> 
        where T : IGroupProcessor
    {
        public int GroupId { get; }
        public void Add(T val, RangedFunction[] funcs);
        public T GetValue(int id);
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
    }
}

