#define USE_LOGS

using Codice.Client.Common.Tree;
using log4net.Config;
using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Base class for all interpolation groups
    /// </summary>
    /// <typeparam name="TStruct"></typeparam>
    /// <typeparam name="TBaseType"></typeparam>
    public abstract class GroupBase<TStruct, TBaseType> : IProcessGroup<TStruct, TBaseType>
    where TStruct : unmanaged
    where TBaseType : unmanaged, IInterpolator<TStruct>
    {
        protected NativeArray<TBaseType> _processors;
        protected NativeArray<EventData<TStruct>> _events;
        protected NativeHashMap<int, int> _lookup;
        protected NativeQueue<int> _removeQue;
        protected RemoveJob _batchRemover;
        protected NativeQueue<int> _addQue;

        private InterpolationJob _processJob;
        private NativeArray<NativeFunctionGraph> _graphHeap;
        private bool _isActive;

        protected delegate void InterpolationDelegate(in TBaseType processor, in NativeFunctionGraph graph, ref ValueVector<TStruct> val, ref ExecutionContext ctx);

        private readonly static FunctionPointer<InterpolationDelegate> s_mainFallback = BurstCompiler.CompileFunctionPointer<InterpolationDelegate>(MainFallback);
        private int _groupId;

        /// <summary>
        /// Is the processor type storing multiple graphs in a single native function graph?
        /// </summary>
        protected virtual bool IsMultiGraphTarget => false;

        /// <summary>
        /// Main interpolation function call
        /// </summary>
        protected virtual FunctionPointer<InterpolationDelegate> MainFunction { get => s_mainFallback; }

        public int ProcAllocSize => _processors.Length;

        int IProcessGroupHandle.GroupId { get => _groupId; set => _groupId = value; }

        private static void MainFallback(in TBaseType proc, in NativeFunctionGraph graph, ref ValueVector<TStruct> val, ref ExecutionContext ctx)
        {
            var func = graph.GetFunction(ctx.Progress);
            val.Current = proc.Interpolate(val.Start, val.End, func, ctx.Progress);
        }

        public GroupBase(int preallocSize)
        {
            _processors = new NativeArray<TBaseType>(preallocSize, Allocator.Persistent);
            _events = new NativeArray<EventData<TStruct>>(preallocSize, Allocator.Persistent);
            _lookup = new NativeHashMap<int, int>(preallocSize, Allocator.Persistent);
            _removeQue = new NativeQueue<int>(Allocator.Persistent);
            _addQue = new NativeQueue<int>(Allocator.Persistent);
            _graphHeap = new NativeArray<NativeFunctionGraph>(preallocSize, Allocator.Persistent);

            // Enques all prealloc indexes to be available
            for(int i = 0; i < preallocSize; i++)
            {
                _addQue.Enqueue(i);
            }
            _isActive = true;
        }

        /// <summary>
        /// Processes all interpolation logic
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle Process(PluginValueCache cache, ContextQueryResults query, JobHandle dep)
        {
            
            // Process internal has to be called before removing or calling listeners
            // but it should be possible to combine all three processes into one 
            // if the callback delegates could be used via pointers. This is slightly
            // problematic due to unity objects behaviour after destroy calls but
            // C# CLR objects should have no problems as long as the object reference itself
            // exists inside QuickActions query, hence pinning it from GC

            _processJob = new InterpolationJob
            {
                Events = _events,
                GraphHeap = query.Graphs,
                Contexts = query.Contexts,
                PluginValueCache = cache,
                MainFunction = MainFunction
            };

            return _processJob.Schedule(dep);

            //foreach (var idIndexPair in _lookup)
            //{
            //    var proc = _processors[idIndexPair.Value];
            //    var evt = _events[idIndexPair.Value];
            //    if (evt.Id != -1)
            //    {
            //        if (!CallbackRegistry.TryCall(evt))
            //            proc.Status = ExecutionStatus.Completed;
            //    }
            //    if (proc.Status == ExecutionStatus.Completed)
            //    {
            //        CallbackRegistry.UnregisterCallbacks(proc.Id);
            //        _removeQue.Enqueue(proc.Id);
            //    }
            ////}
            //if (_removeQue.Count > 0)
            //    RemoveBatched();
            
        }

        /// <summary>
        /// Disposes unmanaged containers
        /// </summary>
        public void Dispose()
        {
            for(int i = 0; i < _graphHeap.Length; i++)
            {
                var graph = _graphHeap[i];
                if(graph.IsCreated)
                    graph.Dispose();
            }
            _graphHeap.Dispose();
            _processors.Dispose();
            _events.Dispose();
            _lookup.Dispose();
            _removeQue.Dispose();
            _addQue.Dispose();
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
                GraphHeap = _graphHeap,
            };
            _batchRemover.Run();
        }

        [BurstCompile]
        public struct RemoveJob : IJob
        {
            public NativeHashMap<int, int> Map;
            public NativeQueue<int> RemoveQue;
            public NativeArray<TBaseType> Processors;
            public NativeQueue<int> AddQue;
            public NativeArray<NativeFunctionGraph> GraphHeap;

            public void Execute()
            {
                while (RemoveQue.TryDequeue(out var id))
                {
                    var index = Map[id];
                    var processor = Processors[index];
                    var graph = GraphHeap[index];
                    graph.Dispose();
                    GraphHeap[index] = graph;
                    //processor.Status = ExecutionStatus.Inactive;
                    //processor.Id = -1;
                    Processors[index] = processor;
                    Map.Remove(id);
                    AddQue.Enqueue(index);
                }
            }
        }

        [BurstCompile]
        protected struct InterpolationJob : IJob
        {
            public PluginValueCache PluginValueCache;

            public NativeArray<EventData<TStruct>> Events;
            public NativeArray<NativeFunctionGraph> GraphHeap;
            public NativeArray<ExecutionContext> Contexts;
            
            public FunctionPointer<InterpolationDelegate> MainFunction;

            public unsafe void Execute()
            {
                var length = PluginValueCache.Capacity;
                for (int i = 0; i < length; i++)
                {
                    var ctx = Contexts[i];
                    
                    if (ctx.Status == ExecutionStatus.Paused || ctx.Status == ExecutionStatus.Inactive)
                    {
                        var eventFlag = Events[i];
                        eventFlag.Id = -1;
                        Events[i] = eventFlag;
                        continue;
                    }
                    var processor = PluginValueCache.GetProcessor<TStruct, TBaseType>(i);
                    var activeFlags = ctx.ActiveFlags;
                    var graph = GraphHeap[i];

                    // Main function call
                    var val = new ValueVector<TStruct>(); // temp
                    MainFunction.Invoke(in processor, in graph, ref val, ref ctx);
                    Events[i] = new EventData<TStruct>(val.Current, ctx, 0);
                }
            }
        }
    }

    [BurstCompile]
    internal struct ContextQuery : IJobParallelFor
    {
        public int GroupReference;
        [ReadOnly] public NativeArray<Process> Pids;
        [ReadOnly] public NativeArray<int> GroupReferences;
        [ReadOnly] public NativeArray<ExecutionContext> Contexts;
        [ReadOnly] public NativeArray<NativeFunctionGraph> Graphs;

        [WriteOnly] public NativeArray<NativeFunctionGraph> ResultGraphs;
        [WriteOnly] public NativeArray<ExecutionContext> ResultContexts;

        public void Execute(int index)
        {
            var gref = GroupReferences[index];
            if (gref == GroupReference)
            {
                var gid = Pids[index].GroupId;
                var graph = Graphs[index];
                var ctx = Contexts[index];

                ResultContexts[gid] = ctx;
                ResultGraphs[gid] = graph;
            }
        }
    }
}

