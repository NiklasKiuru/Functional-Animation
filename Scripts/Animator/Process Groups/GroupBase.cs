#define USE_LOGS

using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Base class for all interpolation groups
    /// </summary>
    /// <typeparam name="TStruct"></typeparam>
    /// <typeparam name="TBaseType"></typeparam>
    public abstract class GroupBase<TStruct, TBaseType> : IProcessGroup<TBaseType>
    where TStruct : unmanaged
    where TBaseType : unmanaged, IInterpolator<TStruct>
    {
        protected NativeArray<TBaseType> _processors;
        protected NativeArray<RangedFunction> _functions;
        protected NativeArray<EventData<TStruct>> _events;
        protected NativeHashMap<int, int> _lookup;
        protected NativeQueue<int> _removeQue;
        protected bool _isActive;
        protected RemoveJob _batchRemover;
        protected NativeQueue<int> _addQue;
        private ProcessFloats _processJob;

        /// <summary>
        /// Group identfier
        /// </summary>
        public abstract int GroupId { get; }

        /// <summary>
        /// Multiplier that organizes the function pointer array properly
        /// </summary>
        protected abstract int Dimension { get; }
        public GroupBase(int preallocSize)
        {
            _processors = new NativeArray<TBaseType>(preallocSize, Allocator.Persistent);
            _functions = new NativeArray<RangedFunction>(preallocSize * EFSettings.MaxFunctions * Dimension, Allocator.Persistent);
            _events = new NativeArray<EventData<TStruct>>(preallocSize, Allocator.Persistent);
            _lookup = new NativeHashMap<int, int>(preallocSize, Allocator.Persistent);
            _removeQue = new NativeQueue<int>(Allocator.Persistent);
            _addQue = new NativeQueue<int>(Allocator.Persistent);

            // Enques all prealloc indexes to be available
            for(int i = 0; i < preallocSize; i++)
            {
                _addQue.Enqueue(i);
            }
            _isActive = true;
        }

        /// <summary>
        /// Adds a new processor into the group
        /// </summary>
        /// <param name="val"></param>
        /// <param name="funcs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TBaseType val, FunctionContainer cont)
        {   
            // No array resizing needed
            if(_addQue.TryDequeue(out var index))
            {
                AddInternal(index, val, cont);
                return;
            }

            // Resize
            var processPos = _processors.Length;
            _processors.ResizeArray(processPos * 2);
            _functions.ResizeArray(_functions.Length * 2);
            _events.ResizeArray(_events.Length * 2);
            AddInternal(processPos, val, cont);
        }

        private void AddInternal(int index, TBaseType val, FunctionContainer cont)
        {
            _processors[index] = val;
            var start = index * EFSettings.MaxFunctions * Dimension;
            var end = start + cont.Length;
            var contIndex = 0;
            for (int i = start; i < end; i++)
            {
                _functions[i] = cont[contIndex];
                contIndex++;
            }
            _lookup.Add(val.InternalId, index);
        }

        /// <summary>
        /// Processes all interpolation logic
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Process()
        {
            _isActive = _lookup.Count() > 0;
            if (_isActive)
            {
                // Process internal has to be called before removing or calling listeners
                // but it should be possible to combine all three processes into one 
                // if the callback delegates could be used via pointers. This is slightly
                // problematic due to unity objects behaviour after destroy calls but native
                // C# classes should have no problems as long as the object reference itself
                // exists inside QuickActions query, hence pinning it from GC
                //var hasEvents = ProcessInternal();
                ProcessInternalJob();
                var len = _processors.Length;
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

                if (_removeQue.Count > 0)
                    RemoveBatched();
            }
        }

        private void ProcessInternalJob()
        {
            _processJob = new ProcessFloats 
            {
                Processors = _processors,
                Functions = _functions,
                Events = _events,
                Delta = Time.deltaTime
            };
            _processJob.Run();
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
            _addQue.Dispose();
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
                Processors = _processors,
                AddQue = _addQue,
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

        void IProcessGroupHandle<IGroupProcessor>.PrecompileJobAssemblies()
        {
            TBaseType temp = default;
            var clock = new Clock(1);
            clock.Tick(1);
            temp.Clock = clock;
            FunctionContainer tempFunc = new FunctionContainer(Dimension);
            tempFunc.Add(0, 0, new RangedFunction(Function.Linear));
            var handle = EFAnimator.RegisterTarget<TStruct, TBaseType>(temp, tempFunc);
            handle.OnStart(this, (v) => LogCalls(handle, "OnStartInit: "))
                .OnUpdate(this, (v) => LogCalls(handle, "OnUpdateInit: "))
                .OnComplete(this, (v) => LogCalls(handle, "OnCompleteInit: "))
                .OnPause(this, (v) => LogCalls(handle, "OnPauseInit: "))
                .OnKill(this, (v) => LogCalls(handle, "OnKillInit: "))
                .OnResume(this, (v) => LogCalls(handle, "OnResumeInit: "))
                .Pause()
                .Resume();
            Process();
            static void LogCalls(IInterpolatorHandle<TStruct> baseT, string message)
            {
#if USE_LOGS
                Debug.Log(message + "GroupId: " + baseT.GetGroupId() + " ProcessId: " + baseT.Id);
#endif
            }
        }

        /// <summary>
        /// Restarts an existing process if available
        /// </summary>
        /// <param name="id"></param>
        public void RestartProcess(int id)
        {
            if(_lookup.TryGetValue(id, out var index))
            {
                var processor = _processors[index];
                processor.Restart();
                _processors[index] = processor;
            }
        }

        [BurstCompile]
        public struct RemoveJob : IJob
        {
            public NativeHashMap<int, int> Map;
            public NativeQueue<int> RemoveQue;
            public NativeArray<TBaseType> Processors;
            public NativeQueue<int> AddQue;

            public void Execute()
            {
                while (RemoveQue.TryDequeue(out var id))
                {
                    var index = Map[id];
                    var processor = Processors[index];
                    processor.Status = ExecutionStatus.Inactive;
                    processor.InternalId = -1;
                    Processors[index] = processor;
                    Map.Remove(id);
                    AddQue.Enqueue(index);
                }
            }
        }

        [BurstCompile]
        public struct ProcessFloats : IJob
        {
            public NativeArray<TBaseType> Processors;
            public NativeArray<EventData<TStruct>> Events;
            public NativeArray<RangedFunction> Functions;
            public float Delta;

            public void Execute()
            {
                var length = Processors.Length;
                for (int i = 0; i < length; i++)
                {
                    var dataPoint = Processors[i];
                    if (dataPoint.Status == ExecutionStatus.Paused || dataPoint.Status == ExecutionStatus.Inactive)
                    {
                        var eventFlag = Events[i];
                        eventFlag.Id = -1;
                        Events[i] = eventFlag;
                        continue;
                    }

                    if (dataPoint.Clock.Time == 0 && (dataPoint.PassiveFlags & EventFlags.OnStart) == EventFlags.OnStart)
                        dataPoint.ActiveFlags |= EventFlags.OnStart;
                    var clock = dataPoint.Clock;
                    var time = clock.Tick(Delta);
                    dataPoint.Clock = clock;
                    var startingPoint = i * EFSettings.MaxFunctions;
                    for (int axis = 0; axis < dataPoint.AxisCount; axis++)
                    {
                        if (!dataPoint.IsValid(axis))
                            continue;
                        startingPoint += axis * EFSettings.MaxFunctions;
                        var endingPoint = startingPoint + dataPoint.PointerCount(axis);
                        for (int j = startingPoint; j < endingPoint; j++)
                        {
                            var rangedFunc = Functions[j];
                            var startingNode = rangedFunc.Start;
                            var endingNode = rangedFunc.End;
                            if (time >= startingNode.x && time <= endingNode.x)
                            {
                                dataPoint.SetValue(axis, rangedFunc.Interpolate(dataPoint.ReadFrom(axis), dataPoint.ReadTo(axis), time));
                                if ((dataPoint.PassiveFlags & EventFlags.OnUpdate) == EventFlags.OnUpdate)
                                    dataPoint.ActiveFlags |= EventFlags.OnUpdate;
                                break;
                            }
                        }

                    }
                    if (dataPoint.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                    {
                        if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete)
                            dataPoint.ActiveFlags |= EventFlags.OnComplete;
                        dataPoint.Status = ExecutionStatus.Completed;
                    }
                    if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete &&
                        dataPoint.Status == ExecutionStatus.Completed)
                        dataPoint.ActiveFlags |= EventFlags.OnComplete;

                    var hasActiveFlags = dataPoint.ActiveFlags != EventFlags.None;
                    if (dataPoint.Status == ExecutionStatus.Completed)
                        dataPoint.ActiveFlags |= EventFlags.OnKill;
                    var flagIndexer = Events[i];
                    if (hasActiveFlags)
                    {
                        flagIndexer.Id = dataPoint.InternalId;
                        flagIndexer.Flags = dataPoint.ActiveFlags;
                        flagIndexer.Value = dataPoint.Current;
                    }
                    else
                    {
                        flagIndexer.Id = -1;
                    }
                    dataPoint.ActiveFlags = EventFlags.None;
                    Processors[i] = dataPoint;
                    Events[i] = flagIndexer;
                }
            }
        }
    }
}

