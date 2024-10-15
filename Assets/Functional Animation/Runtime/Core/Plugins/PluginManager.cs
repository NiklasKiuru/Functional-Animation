using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Pool;

namespace Aikom.FunctionalAnimation
{
    internal class PluginManager
    {
        private Dictionary<int, IProcessGroupHandle> _plugins;
        private List<IProcessGroupHandle> _processGroups;

        public void RegisterPlugin<TPlugin, TStruct, TProcessor>(TPlugin plugin)
            where TPlugin : IProcessGroup<TStruct, TProcessor>
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            if(plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            var groupId = plugin.GroupId;
            if (_plugins.ContainsKey(groupId))
                throw new PluginException();
            _plugins.Add(groupId, plugin);
            _processGroups.Add(plugin);
            var id = ProcessCache.CreatePluginCache<TStruct, TProcessor>(64);
            plugin.GroupId = id;
        }

        public void Process()
        {
            var queryResults = ListPool<ContextQueryResults>.Get();
            var handles = new NativeArray<JobHandle>(_processGroups.Count * 2, Allocator.Persistent);
            for(int i = 0; i < _processGroups.Count; i++)
            {
                var group = _processGroups[i];
                var cache = ProcessCache.GetCache(group);

                var results = new ContextQueryResults()
                {
                    Contexts = new NativeArray<ExecutionContext>(cache.Capacity, Allocator.Persistent),
                    Graphs = new NativeArray<NativeFunctionGraph>(cache.Capacity, Allocator.Persistent),
                };

                var queryJob = new ContextQuery
                {
                    GroupReference = group.GroupId,

                    Pids = ProcessCache.Pids,
                    Contexts = ProcessCache.Contexts,
                    GroupReferences = ProcessCache.Groupreferences,
                    Graphs = ProcessCache.FunctionHeap,

                    ResultContexts = results.Contexts,
                    ResultGraphs = results.Graphs,
                };

                var handle = queryJob.Schedule(ProcessCache.MaxCount, ProcessCache.MaxCount / 8);
                var valueHandle = group.Process(cache, results, handle);

                handles[i * 2] = handle;
                handles[i * 2 + 1] = valueHandle;
            }
            JobHandle.CompleteAll(handles);

            for(int i = 0; i < queryResults.Count; i++)
            {
                var result = queryResults[i];
                result.Graphs.Dispose();
                result.Contexts.Dispose();
            }
            ListPool<ContextQueryResults>.Release(queryResults);
            handles.Dispose();
        }

        public void Dispose()
        {
            foreach( var plugin in _plugins.Values)
                plugin.Dispose();
        }
    }

    public struct ContextQueryResults
    {
        public NativeArray<ExecutionContext> Contexts;
        public NativeArray<NativeFunctionGraph> Graphs;
    }
}
