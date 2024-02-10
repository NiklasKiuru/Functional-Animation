using System.Collections.Generic;
using UnityEngine;
using Aikom.FunctionalAnimation.Utility;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public class EFAnimator : MonoBehaviour
    {
        internal static EFAnimator Instance => (ApplicationUtils.IsQuitting || _instance != null) ? _instance : (_instance = CreateInstance());
        protected static EFAnimator _instance;
        private Dictionary<int, GroupController> _transformGroups = new();
        private Dictionary<int, TransformInterpolationData> _transformData = new();

        private static EFAnimator CreateInstance()
        {
            var gameObject = new GameObject(nameof(EFAnimator))
            {
                hideFlags = HideFlags.DontSave,
            };
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            else
#endif
            {
                DontDestroyOnLoad(gameObject);
            }
            var instance = gameObject.AddComponent<EFAnimator>();
            instance.OnInit();
            return instance;
        }

        internal static IInterpolator<float3> RegisterVectorTarget(TransformInterpolationData data, Transform transform)
        {
            //_monoGroup.Add(data, range, speed, ctrl);
            return default;
        }

        internal static IInterpolator<float> RegisterFloatTarget(FloatInterpolator data, params RangedFunction[] funcs)
        {   
            if(data.Length != funcs.Length)
                throw new System.ArgumentException("Length of data and functions must be equal");
            //_monoGroup.Add(data, range, speed, ctrl);
            return data;
        }
        

        internal static TransformInterpolationData GetData(int hash)
        {   
            var instance = Instance;
            if(instance._transformData.ContainsKey(hash))
            {
                return instance._transformData[hash];
            }
            return new TransformInterpolationData() { IsActive = false };
        }

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
            if(_instance == null)
                _instance = CreateInstance();
            var hash = groupName.GetHashCode();
            if(!_instance._transformGroups.ContainsKey(hash))
            {   
                var virtualParent = new GameObject(groupName);
                preAllocation ??= new List<Transform>(0);
                var controller = new GroupController(threadCount);
                controller.SetAnimation(anim, virtualParent.transform, preAllocation);
                _instance._transformGroups.Add(hash, controller);
            }
        }

        /// <summary>
        /// Terminates an entire group of synchronized transforms.
        /// </summary>
        /// <param name="groupName"></param>
        public static void TerminateTransformGroup(string groupName)
        {
            if(_instance == null)
                return;
            var hash = groupName.GetHashCode();
            if(_instance._transformGroups.TryGetValue(hash, out var controller))
            {
                controller.Disable();
                _instance._transformGroups.Remove(hash);
            }
        }

        /// <summary>
        /// Adds a transform to a synchronized group
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="groupName"></param>
        public static void AddToTransformGroup(Transform transform, string groupName)
        {
            if (_instance == null)
                return;
            var hash = groupName.GetHashCode();
            if(_instance._transformGroups.TryGetValue(hash, out var controller))
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
            if(_instance == null)
                return;
            var hash = groupName.GetHashCode();
            if(_instance._transformGroups.ContainsKey(hash))
            {
                _instance._transformGroups[hash].RemoveGroupChild(transform);
            }
        }

        private void Update()
        {
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
        }


        private void OnInit()
        {
            Application.quitting += Dispose;
        }
        private void Dispose()
        {   
            Destroy(gameObject);
            Application.quitting -= Dispose;
        }

    }
}
