using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Base class for all interpolation groups
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TStruct"></typeparam>
    /// <typeparam name="TBaseType"></typeparam>
    public abstract class GroupBase<TInterface, TStruct, TBaseType> : IProcessGroup<TBaseType>
    where TInterface : IInterpolator<TStruct>, IGroupProcessor
    where TStruct : unmanaged
    where TBaseType : unmanaged, IInterpolator<TStruct>, TInterface
    {
        protected NativeList<TBaseType> _processors;
        protected NativeList<RangedFunction> _functions;
        protected NativeList<FlagIndexer<TStruct>> _events;
        protected NativeHashMap<int, int> _lookup;
        protected NativeQueue<int> _removeQue;
        protected bool _isActive;
        protected RemoveJob _batchRemover;

        /// <summary>
        /// Group identfier
        /// </summary>
        public abstract int GroupId { get; }

        public GroupBase(int preallocSize)
        {
            _processors = new NativeList<TBaseType>(preallocSize, Allocator.Persistent);
            _functions = new NativeList<RangedFunction>(preallocSize, Allocator.Persistent);
            _events = new NativeList<FlagIndexer<TStruct>>(preallocSize, Allocator.Persistent);
            _lookup = new NativeHashMap<int, int>(preallocSize, Allocator.Persistent);
            _removeQue = new NativeQueue<int>(Allocator.Persistent);
            _isActive = true;
        }

        /// <summary>
        /// Adds a new processor into the group
        /// </summary>
        /// <param name="val"></param>
        /// <param name="funcs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TBaseType val, RangedFunction[] funcs)
        {
            _processors.Add(val);
            for (int i = 0; i < funcs.Length; i++)
            {
                _functions.Add(funcs[i]);
            }
            _events.Add(new FlagIndexer<TStruct> { Id = -1, Flags = EventFlags.None, Value = default });
            _lookup.Add(val.InternalId, _processors.Length - 1);
        }

        /// <summary>
        /// Processes all interpolation logic
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process()
        {
            _isActive = _processors.Length > 0;
            if (_isActive)
            {   
                // Process internal has to be called before removing or calling listeners
                // but it should be possible to combine all three processes into one 
                // if the callback delegates could be used via pointers. This is slightly
                // problematic due to unity objects behaviour after destroy calls but native
                // C# classes should have no problems as long as the object reference itself
                // exists inside QuickActions query, hence pinning it from GC
                var hasEvents = ProcessInternal();
                if (hasEvents)
                {   
                    var len = _processors.Length;   // Surprisingly fast when caching the length
                    for (int i = 0; i < len; i++)
                    {
                        var proc = _processors[i];
                        var evt = _events[i];
                        if (evt.Id != -1)
                            CallbackRegistry.TryCall(evt);
                        if (proc.Status == ExecutionStatus.Completed)
                        {
                            CallbackRegistry.UnregisterCallbacks(proc.InternalId);
                            _removeQue.Enqueue(proc.InternalId);
                        }
                    }
                }
                if (_removeQue.Count > 0)
                    RemoveBatched();
            }
        }

        /// <summary>
        /// Disposes unmanaged containers
        /// </summary>
        public void Dispose()
        {
            _functions.Dispose();
            _processors.Dispose();
            _events.Dispose();
            _lookup.Dispose();
            _removeQue.Dispose();
        }

        protected unsafe abstract bool ProcessInternal();

        /// <summary>
        /// Sets passive flags for a process
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flags"></param>
        public void SetPassiveFlags(int id, EventFlags flags)
        {
            if(_lookup.TryGetValue(id, out var index))
            {   
                var processor = _processors[index];
                processor.PassiveFlags |= flags;
                _processors[index] = processor;
            }
        }

        /// <summary>
        /// Forces the execution status on a process
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        public void ForceExecutionStatus(int id, ExecutionStatus status)
        {
            if (_lookup.TryGetValue(id, out var index))
            {
                var processor = _processors[index];
                processor.Status = status;
                _processors[index] = processor;
            }
        }

        /// <summary>
        /// Removes a running process from the process list
        /// </summary>
        /// <param name="id"></param>
        public void ForceRemove(int id)
        {
            if(_lookup.ContainsKey(id))
            {
                _removeQue.Enqueue(id);
                RemoveBatched();
            }
        }

        /// <summary>
        /// Removes all qued remove IDs in a job
        /// </summary>
        private void RemoveBatched()
        {
            _batchRemover = new RemoveJob 
            {
                Map = _lookup,
                RemoveQue = _removeQue,
                Functions = _functions,
                Processors = _processors,
                Events = _events,
            };
            _batchRemover.Run();
        }

        /// <summary>
        /// Gets a current execution value of a process
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TBaseType GetValue(int id)
        {
            if(_lookup.TryGetValue(id, out var index))
            {
                return _processors[index];
            }
            return default;
        }

        public interface IRemoveJob : IJob
        {
            public NativeList<TBaseType> Processors { get; set; }
            public NativeList<FlagIndexer<TStruct>> Events { get; set; }
        }

        [BurstCompile]
        public struct RemoveJob : IRemoveJob
        {
            public NativeHashMap<int, int> Map;
            public NativeQueue<int> RemoveQue;
            public NativeList<RangedFunction> Functions;
            public NativeList<TBaseType> Processors { get; set; }
            public NativeList<FlagIndexer<TStruct>> Events { get; set; }

            public void Execute()
            {
                while (RemoveQue.TryDequeue(out var id))
                {
                    var index = 0;
                    var end = 0;
                    var start = 0;
                    var len = Processors.Length;

                    for (int i = 0; i < len; i++)
                    {
                        var data = Processors[i];
                        if (data.InternalId == id)
                        {
                            index = i;
                            end = start + data.Length;
                            break;
                        }
                        start += data.Length;
                    }
                    for (int i = start; i < end; i++)
                    {
                        Functions.RemoveAtSwapBack(i);
                    }

                    Processors.RemoveAtSwapBack(index);
                    Events.RemoveAtSwapBack(index);
                    Map.Remove(id);

                    for (int i = 0; i < len - 1; i++)
                    {
                        var processor = Processors[i];
                        if (Map[processor.InternalId] > index)
                            Map[processor.InternalId] -= 1;
                    }
                }
            }
        }
    }

    

    
}

