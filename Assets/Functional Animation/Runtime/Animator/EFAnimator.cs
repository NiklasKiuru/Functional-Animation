#undef USE_INDEX_SAFEGUARDS

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Central controller for value interpolations and transform controls
    /// </summary>
    public class EFAnimator : MonoBehaviour
    {
        private static EFAnimator _instance;
        private static Dictionary<int, GroupController> _transformGroups = new();
        private static PluginManager _pluginManager;
        private TimeJob _timeJob;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            if(_instance != null) 
                return;

            // Load runtime function cache
            BurstFunctionCache.Load();
            
            // Create singleton
            var gameObject = new GameObject(nameof(EFAnimator));
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<EFAnimator>();
            _instance.OnInit();
        }

        #region Interpolation controls

        public static void RegisterPlugin<TPlugin, TStruct, TProcessor>(TPlugin group)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            where TPlugin : IProcessGroup<TStruct, TProcessor>
        {
            if(group == null)
                throw new ArgumentNullException(nameof(group));
            if (_instance == null)
                CreateInstance();

            try
            {
                _pluginManager.RegisterPlugin<TPlugin, TStruct, TProcessor>(group);
            }
            catch (PluginException e)
            {
                Debug.LogError(e);
            }
        }

        internal static void RegisterStaticCallback<TStruct, TProcessor>(IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> cb, EventFlags flags)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            RegisterCallback(handle, _instance, cb, flags);
        }

        internal static void RegisterStaticCallback<T>(Process id, Action<T> cb, EventFlags flags)
            where T : unmanaged
        {
            ref ExecutionContext ctx = ref ProcessCache.GetContext(id);
            ctx.PassiveFlags |= flags;
            CallbackRegistry.RegisterCallback(id.Id, cb, _instance, flags);
        }

        internal static void RegisterInstancedCallback<TStruct, TProcessor, TObject>(IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> cb, EventFlags flags)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            where TObject : UnityEngine.Object
        {
            RegisterCallback(handle, owner, cb, flags);
        }

        private static void RegisterCallback<TStruct, TProcessor, TObject>(IInterpolatorHandle<TStruct, TProcessor> handle, TObject obj, Action<TStruct> cb, EventFlags flags)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            where TObject : UnityEngine.Object
        {
            ref ExecutionContext ctx = ref ProcessCache.GetContext(handle.ProcessId);
            ctx.PassiveFlags |= flags;
            CallbackRegistry.RegisterCallback(handle.ProcessId.Id, cb, obj, flags);
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
            // Run timers
            _timeJob = new TimeJob
            {
                Clocks = ProcessCache.Clocks,
                Contexts = ProcessCache.Contexts,
                Delta = Time.deltaTime
            };
            _timeJob.Run();

            // Run processes
            _pluginManager.Process();
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
            _pluginManager.Dispose();
            ProcessCache.Destroy();
        }

        /// <summary>
        /// Initialization. 
        /// We could pretermine the preallocation sizes with some global settings in the future
        /// This would also allow better pool management if necessary
        /// </summary>
        private void OnInit()
        {
            var prealloc = EFSettings.GroupAllocationSize;
            CallbackRegistry.Prime(prealloc);
            ProcessCache.Create(prealloc * 4);
            _pluginManager = new();

            var floatGroup = new FloatGroup(prealloc);
            var float2Group = new Float2Group(prealloc);
            var float3Group = new Float3Group(prealloc);
            var float4Group = new Float4Group(prealloc);

            _pluginManager.RegisterPlugin<FloatGroup, float, FloatInterpolator>(floatGroup);
            _pluginManager.RegisterPlugin<Float2Group, float2, Vector2Interpolator>(float2Group);
            _pluginManager.RegisterPlugin<Float3Group, float3, Vector3Interpolator>(float3Group);
            _pluginManager.RegisterPlugin<Float4Group, float4, Vector4Interpolator>(float4Group);

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

