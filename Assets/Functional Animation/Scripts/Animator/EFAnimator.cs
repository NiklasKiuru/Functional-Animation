#undef USE_INDEX_SAFEGUARDS

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Central controller for value interpolations and transform controls
    /// </summary>
    /// <remarks>Note that all methods expecting a type of IInterpolator<T> are internal calls where the basetype must be known.
    /// Since no basetypes are implicitly exposed to the user these calls should only be possible to do internally
    /// This allows bypassing all process IsAlive checks for brute force control over the process
    /// Due to these liberties all internal process modification calls must ensure Id validity with either
    /// explicitly controlled lifetime settings or kill commands</remarks>
    public class EFAnimator : MonoBehaviour
    {
        private static EFAnimator _instance;
        private static Dictionary<int, GroupController> _transformGroups = new();
        private static IndexPool _indexer;
        private static Dictionary<int, IProcessGroupHandle<IGroupProcessor>> _processGroups;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var gameObject = new GameObject(nameof(EFAnimator));
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<EFAnimator>();
            _instance.OnInit();
        }

        #region Interpolation controls
        /// <summary>
        /// Registers an interpolator target into a group controller
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="processor"></param>
        /// <param name="funcs"></param>
        /// <returns>A new <see cref="HandleTracker{T}"/> object. Allocates memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IInterpolatorHandle<T> RegisterTarget<T, D>(D processor, FunctionContainer cont)
            where T : unmanaged
            where D : IInterpolator<T>
        {   
            var groupId = processor.GetGroupId();
            processor.Id = _indexer.GetNewId();
            processor.Status = ExecutionStatus.Running;
            processor.IsAlive = true;
            IInterpolatorHandle<T> wrapper = new HandleTracker<T>(processor, cont);
            if (TryGetExplicitGroup<T, D>(groupId, out var group))
            {
                group.Add(processor, cont);
                wrapper.OnKill(_instance, (v) =>
                {
                    wrapper.IsAlive = false;
#if USE_INDEX_SAFEGUARDS
                    _indexer.Return(processor.InternalId);
#endif
                });
            }
            else
            {
                wrapper.IsAlive = false;
            }
            return wrapper;
        }

        /// <summary>
        /// Gets the active processor from a group if there is any
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="proc"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetProcessor<T, D>(ProcessId proc, out D process)
            where T : unmanaged
            where D : IInterpolator<T>
        {
            process = default;
            if(TryGetExplicitGroup<T, D>(proc.GroupId, out var group))
            {
                process = group.GetValue(proc.Id);
                if(process.Id == proc.Id)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Registers a target internally with no new memory allocations. For this to be truly non alloc the container must be passed with a using statement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="processor"></param>
        /// <param name="cont"></param>
        /// <returns>The newly registered process id</returns>
        internal static ProcessId RegisterTargetNonAlloc<T, D>(ref D processor, FunctionContainer cont)
            where T : unmanaged
            where D : IInterpolator<T>
        {
            if (TryGetExplicitGroup<T, D>(processor.GetGroupId(), out var group))
            {
                processor.Id = _indexer.GetNewId();
                processor.Status = ExecutionStatus.Running;
                processor.IsAlive = true;
                group.Add(processor, cont);
#if USE_INDEX_SAFEGUARDS
                processor.OnKill(_instance, (v) =>
                {
                    _indexer.Return(processor.InternalId);
                });
#endif
                return processor.GetIdentifier();
            }
            return new ProcessId(-1, -1);
        }

        /// <summary>
        /// Helper method for setting execution status with an external handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="status"></param>
        public static void ForceExecutionStatusExternal<T>(ref IInterpolatorHandle<T> handle, ExecutionStatus status)
            where T : unmanaged
        {
            if(TryGetValidGroup(handle, out var group))
            {
                group.ForceExecutionStatus(handle.Id, status);
                handle.IsAlive = status != ExecutionStatus.Completed;
            }
        }

        /// <summary>
        /// Helper method for setting passive event flags with an external handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="flag"></param>
        public static void SetPassiveFlagsExternal<T>(ref IInterpolatorHandle<T> handle, EventFlags flag)
            where T : unmanaged
        {
            if(TryGetValidGroup(handle, out var group))
            {
                group.SetPassiveFlags(handle.Id, flag);
            }
        }

        /// <summary>
        /// Sets event flags bypassing alive checks
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="processId"></param>
        /// <param name="flags"></param>
        internal static void SetPassiveFlagsInternal(ProcessId proc, EventFlags flags)
        {
            if(_processGroups.TryGetValue(proc.GroupId, out var group))
            {
                group.SetPassiveFlags(proc.Id, flags);
            }
        }

        /// <summary>
        /// Helper method to remove a target with an external handle. The process has to be alive for this to complete
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static void KillTargetExternal<T>(ref IInterpolatorHandle<T> handle) where T : unmanaged
        {
            if (TryGetValidGroup(handle, out var group))
            {
                CallbackRegistry.TryCall(new EventData<T>() { Id = handle.Id, Flags = EventFlags.OnKill, Value = handle.GetValue() });
                CallbackRegistry.UnregisterCallbacks(handle.Id);
                handle.IsAlive = false;
                group.ForceRemove(handle.Id);
            }
        }

        /// <summary>
        /// Helper method to get the current execution value with an external handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        internal static T GetValueExternal<T, D>(D handle) 
            where T : unmanaged
            where D : IInterpolator<T>
        {
            if (TryGetProcessor<T, D>(handle.GetIdentifier(), out var processor)) 
            {   
                return processor.Current;
            }
            return default;
        }

        /// <summary>
        /// Kills the target when only the basetype is known. This will bypass all IsAlive checks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="processId"></param>
        internal static void KillTargetInternal<T, D>(int groupId, int processId)
            where T : unmanaged
            where D : IInterpolator<T>
        {
            if(TryGetExplicitGroup<T, D>(groupId, out var group))
            {
                var val = group.GetValue(processId).Current;
                CallbackRegistry.TryCall(new EventData<T>() { Id = processId, Flags = EventFlags.OnKill, Value = val });
                CallbackRegistry.UnregisterCallbacks(processId);
                group.ForceRemove(processId);
            }
        }

        /// <summary>
        /// Sets max loop count of a process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="count"></param>
        public static void SetMaxLoopCountExternal<T>(ref IInterpolatorHandle<T> handle, int count)
            where T : unmanaged
        {
            if(TryGetValidGroup(handle, out var group))
            {
                group.SetMaxLoopCount(handle.Id, count);
            }
        }

        /// <summary>
        /// Sets max loop count of a process
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="count"></param>
        internal static void SetMaxLoopCountInternal(ProcessId proc, int count)
        {
            var group = _processGroups[proc.GroupId];
            group.SetMaxLoopCount(proc.Id, count);
        }

        /// <summary>
        /// Restarts a process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static void RestartProcess<T>(IInterpolatorHandle<T> handle)
            where T : unmanaged
        {   
            // Handle is still alive
            if (TryGetValidGroup(handle, out var group))
            {
                group.RestartProcess(handle.Id);
            }
            else if(handle is HandleTracker<T> tracker)
            {   
                // Does nonAlloc internally so we need a new kill command
                tracker.Restart();
                tracker.OnKill(_instance, (v) => handle.IsAlive = false);
            }
        }

        /// <summary>
        /// Gets a valid group from a handle if the handle is alive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private static bool TryGetValidGroup<T>(IInterpolatorHandle<T> handle, out IProcessGroupHandle<IGroupProcessor> group)
            where T : unmanaged
        {   
            group = null;
            if (handle.IsAlive && _processGroups.TryGetValue(handle.GetGroupId(), out group))
                return true;
            return false;
        }

        /// <summary>
        /// Gets a valid explicitly defined group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="groupId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        private static bool TryGetExplicitGroup<T, D>(int groupId, out IProcessGroup<D> group)
            where T : unmanaged
            where D : IInterpolator<T>
        {
            group = null;
            if(_processGroups.TryGetValue(groupId, out var handle))
            {
                group = handle as IProcessGroup<D>;
                if(group != null) 
                    return true;
            }
            return false;
        }
        
        #endregion

        #region Transform controllers
        /// <summary>
        /// Creates a new synchronized transform group where each target gets animated by the same animation.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="anim"></param>
        /// <param name="threadCount"></param>
        /// <param name="preAllocation"></param>
        public static void CreateTransformGroup(string groupName, TransformAnimation anim, int threadCount, List<Transform> preAllocation)
        {   
            if(anim == null || string.IsNullOrEmpty(groupName))
                return;

            var hash = groupName.GetHashCode();
            if(!_transformGroups.ContainsKey(hash))
            {   
                var virtualParent = new GameObject(groupName);
                preAllocation ??= new List<Transform>(0);
                var controller = new GroupController(threadCount);
                controller.Start(anim, virtualParent.transform, preAllocation);
                _transformGroups.Add(hash, controller);
            }
        }

        /// <summary>
        /// Terminates an entire group of synchronized transforms.
        /// </summary>
        /// <param name="groupName"></param>
        public static void TerminateTransformGroup(string groupName)
        {
            var hash = groupName.GetHashCode();
            if(_transformGroups.TryGetValue(hash, out var controller))
            {
                controller.Disable();
                _transformGroups.Remove(hash);
            }
        }

        /// <summary>
        /// Adds a transform to a synchronized group
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="groupName"></param>
        public static void AddToTransformGroup(Transform transform, string groupName)
        {
            var hash = groupName.GetHashCode();
            if(_transformGroups.TryGetValue(hash, out var controller))
            {
                controller.AddGroupChild(transform);
            }
        }

        /// <summary>
        /// Removes a transform from a synchronized group
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="groupName"></param>
        public static void RemoveFromTransformGroup(Transform transform, string groupName)
        {   
            var hash = groupName.GetHashCode();
            if(_transformGroups.ContainsKey(hash))
            {
                _transformGroups[hash].RemoveGroupChild(transform);
            }
        }
        #endregion

        #region Instance methods
        private void Update()
        {   
            foreach(var group in _processGroups.Values)
            {
                group.Process();
            }

            foreach (var group in _transformGroups)
            {
                group.Value.Update();
            }
            
        }

        private void LateUpdate()
        {
            foreach (var group in _transformGroups)
            {
                group.Value.CompleteJobs();
            }
        }

        private void OnDestroy()
        {
            if (_transformGroups != null)
            {
                foreach (var group in _transformGroups)
                {
                    group.Value.Dispose();
                }
                _transformGroups.Clear();
            }
            foreach(var group in _processGroups.Values)
            {
                group.Dispose();
            }
        }

        /// <summary>
        /// Initialization. 
        /// We could pretermine the preallocation sizes with some global settings in the future
        /// This would also allow better pool management if necessary
        /// </summary>
        private void OnInit()
        {
            var prealloc = EFSettings.GroupAllocationSize;
            _indexer = new(prealloc);
            _processGroups = new();
            CallbackRegistry.Prime(prealloc);

            IProcessGroup<FloatInterpolator> floatGroup = new FloatGroup(prealloc);
            IProcessGroup<Vector2Interpolator> float2Group = new Float2Group(prealloc);
            IProcessGroup<Vector3Interpolator> float3Group = new Float3Group(prealloc);
            IProcessGroup<Vector4Interpolator> float4Group = new Float4Group(prealloc);

            _processGroups.Add(floatGroup.GroupId, floatGroup);
            _processGroups.Add(float2Group.GroupId, float2Group);
            _processGroups.Add(float3Group.GroupId, float3Group);
            _processGroups.Add(float4Group.GroupId, float4Group);

            // Circumvents JIT compiler from having to compile in runtime
            // This will prevent framedrops in first Add() or Remove() calls in process groups
#if !UNITY_EDITOR
            foreach (var group in _processGroups.Values)
            {
                group.PrecompileJobAssemblies();
            }
#endif
            Application.quitting += Dispose;
        }
        private void Dispose()
        {   
            Destroy(gameObject);
            Application.quitting -= Dispose;
        }
#endregion
    }
}

