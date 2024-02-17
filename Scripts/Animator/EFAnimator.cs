#undef USE_INDEX_SAFEGUARDS

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Central controller for value interpolations and transform controls
    /// </summary>
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
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IInterpolatorHandle<T> RegisterTarget<T, D>(D processor, FunctionContainer cont)
            where T : unmanaged
            where D : IInterpolator<T>
        {   
            var groupId = processor.GetGroupId();
            processor.InternalId = _indexer.GetNewId();
            processor.Status = ExecutionStatus.Running;
            processor.IsAlive = true;
            IInterpolatorHandle<T> wrapper = new HandleTracker<T>(processor, cont);
            if (_processGroups.TryGetValue(groupId, out var group))
            {
                var implGroup = group as IProcessGroup<D>;  // This is dumb but mandatory
                implGroup.Add(processor, cont);
                processor.OnKill(_instance, (v) =>
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

        internal static void RegisterTargetNonAlloc<T, D>(ref D processor, FunctionContainer cont)
            where T : unmanaged
            where D : IInterpolator<T>
        {
            var groupId = processor.GetGroupId();
            if (_processGroups.TryGetValue(groupId, out var group))
            {
                processor.InternalId = _indexer.GetNewId();
                processor.Status = ExecutionStatus.Running;
                processor.IsAlive = true;
                var implGroup = group as IProcessGroup<D>;  // This is dumb but mandatory
                implGroup.Add(processor, cont);
#if USE_INDEX_SAFEGUARDS
                processor.OnKill(_instance, (v) =>
                {
                    _indexer.Return(processor.InternalId);
                });
#endif
            }
        }

        /// <summary>
        /// Helper method for setting execution status with an external handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="status"></param>
        internal static void ForceExecutionStatusExternal<T>(ref IInterpolatorHandle<T> handle, ExecutionStatus status)
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
        internal static void SetPassiveFlagsExternal<T>(ref IInterpolatorHandle<T> handle, EventFlags flag)
            where T : unmanaged
        {
            if(TryGetValidGroup(handle, out var group))
            {
                group.SetPassiveFlags(handle.Id, flag);
            }
        }

        /// <summary>
        /// Helper method to remove a target with an external handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        internal static void KillTargetExternal<T>(ref IInterpolatorHandle<T> handle) where T : unmanaged
        {
            if (TryGetValidGroup(handle, out var group))
            {
                CallbackRegistry.TryCall(new EventData<T>() { Id = handle.Id, Flags = EventFlags.OnKill, Value = handle.GetValue() });
                CallbackRegistry.UnregisterCallbacks(handle.Id);
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
            if (TryGetValidGroup(handle, out var group)) 
            {   
                var implHandle = group as IProcessGroup<D>;
                return implHandle.GetValue(handle.Id).Current;
            }
            return default;
        }

        internal static void RestartProcess<T>(IInterpolatorHandle<T> handle)
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

        private static bool TryGetValidGroup<T>(IInterpolatorHandle<T> handle, out IProcessGroupHandle<IGroupProcessor> group)
            where T : unmanaged
        {   
            group = null;
            if (handle.IsAlive && _processGroups.TryGetValue(handle.GetGroupId(), out group))
                return true;
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
                controller.SetAnimation(anim, virtualParent.transform, preAllocation);
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
                group.Value.ApplyTransformations();
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
            foreach (var group in _processGroups.Values)
            {
                group.PrecompileJobAssemblies();
            }

            

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

