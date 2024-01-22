using System.Linq;
using UnityEngine;
using System;
using System.Collections;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Base class for all property animators
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AnimatorBase<T> : MonoBehaviour where T : struct
    {
        [Tooltip("If checked will sync the animation duration of all properties")]
        [SerializeField] private bool _syncAll;

        [Tooltip("Max duration of the animation if syncronizing all")]
        [SerializeField] private float _maxDuration;

        [Tooltip("If syncronized this will loop the central time controller")]
        [SerializeField] private bool _loop;

        [Tooltip("Delay before the animation starts playing")]
        [SerializeField] private float _delay;

        private TimeKeeper _timer;
        private Action _animate;

        /// <summary>
        /// Array of property containers that hold the properties to be animated
        /// </summary>
        public abstract PropertyContainer<T>[] ActiveTargets { get; }
        internal TimeKeeper Timer { get => _timer; }
        public bool SyncAll { get => _syncAll; protected set => _syncAll = value; }
        public float MaxDuration { get => _maxDuration; protected set => _maxDuration = value; }
        public bool Loop { get => _loop; protected set => _loop = value; }

        private void Awake()
        {
            Validate();
        }

        private void Start()
        {   
            if(_delay > 0)
            {
                var oldValues = new bool[ActiveTargets.Length];
                for (int i = 0; i < ActiveTargets.Length; i++)
                {
                    oldValues[i] = ActiveTargets[i].Animate;
                    ActiveTargets[i].Animate = false;
                }

                StartCoroutine(StartDelayed(oldValues));
            }
        }

        private void Update()
        {
            _animate?.Invoke();
        }

        protected virtual void OnValidate()
        {
            Validate();
        }

        protected void Validate()
        {
            SetTargets();
            if (_syncAll)
            {
                var duration = 0f;
                foreach (var target in ActiveTargets)
                {
                    if (target != null)
                        duration = Mathf.Max(duration, target.Duration, _maxDuration);
                }
                _maxDuration = duration;
                if(_maxDuration == 0)
                {
                    PauseAll(true);
                    return;
                }
                    
                foreach (var target in ActiveTargets)
                {
                    if (target != null)
                    {
                        target.Syncronize(_maxDuration);
                    }
                }

                _timer = new TimeKeeper(1 / _maxDuration, _loop ? TimeControl.Loop : TimeControl.OneShot);
                _animate = () =>
                {
                    foreach (var target in ActiveTargets)
                    {
                        if (target.Animate)
                        {
                            target.UpdateSync(_timer.Tick() * _maxDuration);
                        }

                    }
                };
            }
            else
            {
                _timer = new TimeKeeper(0);
                _animate = () =>
                {
                    foreach (var target in ActiveTargets)
                    {
                        if (target.Animate)
                            target.Update();
                    }
                };
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
        public virtual void RegisterCallBack(int propContainerIndex, Action<T> callback, EventType evt)
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
        public virtual void UnRegisterCallBack(int propContainerIndex, Action<T> callback, EventType evt)
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

        /// <summary>
        /// Sets the targets for the animator
        /// </summary>
        protected abstract void SetTargets();

        private IEnumerator StartDelayed(bool[] oldValues)
        {
            yield return new WaitForSeconds(_delay);
            for (int i = 0; i < ActiveTargets.Length; i++)
            {
                ActiveTargets[i].Animate = oldValues[i];
            }
        }
    }
}
