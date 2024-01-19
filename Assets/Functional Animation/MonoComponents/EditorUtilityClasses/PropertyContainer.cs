using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public abstract class PropertyContainer<T> where T : struct
    {
        [Tooltip("Should this property be animated?")]
        public bool Animate = true;

        [Tooltip("How should the linear time be managed")]
        public TimeControl TimeControl;

        [Tooltip("Duration of the animation")]
        public float Duration;

        [Tooltip("Target value to animate towards to")]
        public T Target;

        private Action _setValue;

        public event Action<T> OnValueChanged;
        public event Action<T> OnTargetReached;
        public event Action<T> OnStartReached;

        /// <summary>
        /// Updates the property value
        /// </summary>
        public void Update()
        {
            _setValue.Invoke();
        }

        /// <summary>
        /// Creates an interpolator for the property
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="setValue"></param>
        public void CreateInterpolator(T startVal, Action<T> setValue)
        {
            var func = SetEasingFunction();
            var timer = new TimeKeeper(1 / Duration, TimeControl);
            _setValue = () =>
            {   
                var val = IncrimentValue(startVal, timer.Tick(), func);
                setValue(val);
                OnValueChanged?.Invoke(val);
                if (timer.Time == 0)
                    OnStartReached?.Invoke(val);
                if (timer.Time >= 1)
                {
                    OnTargetReached?.Invoke(val);
                    if(TimeControl == TimeControl.OneShot)
                        Animate = false;
                }
            };
        }

        /// <summary>
        /// Sets the easing function for the property animation
        /// </summary>
        /// <returns></returns>
        protected abstract Func<float, float> SetEasingFunction();

        /// <summary>
        /// The method defining how the property value is changed
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="time"></param>
        /// <param name="easingFunc"></param>
        /// <returns></returns>
        protected abstract T IncrimentValue(T startVal, float time, Func<float, float> easingFunc);

    }

    public enum EventType
    {
        OnValueChanged,
        OnTargetReached,
        OnStartReached
    }
}
