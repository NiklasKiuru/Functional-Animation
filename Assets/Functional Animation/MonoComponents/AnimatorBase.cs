using System.Linq;
using UnityEngine;
using System;
using System.Net.Http.Headers;

namespace Aikom.FunctionalAnimation
{
    public abstract class AnimatorBase<T> : MonoBehaviour where T : struct
    {
        [Tooltip("Should all object properties be animated on awake?")]
        [SerializeField] private bool _animateOnAwake = true;

        [Tooltip("If checked will sync the animation duration of all properties")]
        [SerializeField] private bool _syncAll;

        public bool AnimateOnAwake { get => _animateOnAwake; }
        public bool SyncAll { get => _syncAll; }
        public abstract PropertyContainer<T>[] ActiveTargets { get; protected set; }

        private void Awake()
        {
            SetTargets();
            if (_animateOnAwake)
            {
                foreach (var target in ActiveTargets)
                {
                    target.Animate = true;
                }
            }
        }

        private void Update()
        {
            foreach (var target in ActiveTargets)
            {
                if(target.Animate)
                    target.Update();
            }
        }

        protected virtual void OnValidate()
        {
            if (ActiveTargets == null || ActiveTargets.Length == 1)
                SetTargets();
            if(_syncAll)
            {
                foreach(var target in ActiveTargets)
                {
                    var duration = ActiveTargets.Max(t => t.Duration);  // This is just to avoid cases where there is just one null target
                    target.Duration = duration;
                }
            }
        }

        /// <summary>
        /// Pauses or unpauses all transform property animations and enables or disables the component
        /// </summary>
        /// <param name="pause"></param>
        public void PauseAll(bool pause)
        {
            foreach (var target in ActiveTargets)
            {
                target.Animate = !pause;
            }
            enabled = !pause;
        }

        /// <summary>
        /// Pauses or unpauses the animation of a property in stored index
        /// </summary>
        /// <param name="propContainerIndex"></param>
        /// <param name="pause"></param>
        public void Pause(int propContainerIndex, bool pause)
        {
            if(propContainerIndex < ActiveTargets.Length && propContainerIndex > 0)
                ActiveTargets[propContainerIndex].Animate = !pause;

            int activeTargets = 0;
            foreach (var target in ActiveTargets)
            {
                if (target.Animate)
                    activeTargets++;
            }

            enabled = activeTargets > 0;
        }

        /// <summary>
        /// Registers a callback to a property container event in stored index
        /// </summary>
        /// <param name="propContainerIndex"></param>
        /// <param name="callback"></param>
        /// <param name="evt"></param>
        public void RegisterCallBack(int propContainerIndex, Action<T> callback, EventType evt)
        {
            if (propContainerIndex < ActiveTargets.Length && propContainerIndex > 0)
            {
                switch(evt)
                {
                    case EventType.OnValueChanged:
                        ActiveTargets[propContainerIndex].OnValueChanged += callback;
                        break;
                    case EventType.OnTargetReached:
                        ActiveTargets[propContainerIndex].OnTargetReached += callback;
                        break;
                    case EventType.OnStartReached:
                        ActiveTargets[propContainerIndex].OnStartReached += callback;
                        break;
                }
            }
        }

        /// <summary>
        /// Unregisters a callback from a property container event in stored index
        /// </summary>
        /// <param name="propContainerIndex"></param>
        /// <param name="callback"></param>
        /// <param name="evt"></param>
        public void UnRegisterCallBack(int propContainerIndex, Action<T> callback, EventType evt)
        {
            if (propContainerIndex < ActiveTargets.Length && propContainerIndex > 0)
            {
                switch(evt)
                {
                    case EventType.OnValueChanged:
                        ActiveTargets[propContainerIndex].OnValueChanged -= callback;
                        break;
                    case EventType.OnTargetReached:
                        ActiveTargets[propContainerIndex].OnTargetReached -= callback;
                        break;
                    case EventType.OnStartReached:
                        ActiveTargets[propContainerIndex].OnStartReached -= callback;
                        break;
                }
            }
        }

        protected abstract void SetTargets();
    }
}
