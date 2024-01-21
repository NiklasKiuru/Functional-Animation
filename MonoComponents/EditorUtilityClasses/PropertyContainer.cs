using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Aikom.FunctionalAnimation
{
    public abstract class PropertyContainer<T> where T : struct
    {
        [Tooltip("Should this property be animated?")]
        [SerializeField] private bool _animate = true;

        [Tooltip("How should the linear time be managed")]
        [SerializeField] private TimeControl _timeControl;

        [Tooltip("Duration of the animation")]
        [SerializeField] private float _duration;

        [Tooltip("Trim the animation from the beginnning")]
        [SerializeField][Range(0, 1)] private float _trimFront = 0;

        [Tooltip("Trim the animation from the end")]
        [SerializeField][Range(0, 1)] private float _trimBack = 1;

        [Tooltip("Target value to animate towards to")]
        [SerializeField] private T _target;

        /// <summary>
        /// Main function that sets the property value
        /// </summary>
        private Action<float> _setValue;

        private TimeKeeper _timer;
        private float _startTimeStamp;
        private float _endTimeStamp;

        /// <summary>
        /// Invoked once the property value has changed
        /// </summary>
        public event Action<T> OnValueChanged;

        /// <summary>
        /// Invoked once the property value has reached the target value
        /// </summary>
        public event Action<T> OnTargetReached;

        /// <summary>
        /// Invoked once the property value has reached the start value
        /// </summary>
        public event Action<T> OnStartReached;

        /// <summary>
        /// Is this property animated
        /// </summary>
        public bool Animate { get => _animate; set => _animate = value; }
        public float TrimFront { get => _trimFront; protected set => _trimFront = value; }
        public float TrimBack { get => _trimBack; protected set => _trimBack = value; }

        /// <summary>
        /// Time control for the property animation
        /// </summary>
        public TimeControl TimeControl 
        { 
            get => _timeControl;
            set
            {
                _timeControl = value;
                if(_timer == null) 
                    _timer = new TimeKeeper(1 / _duration, _timeControl);
                else
                    _timer.SetTimeControl(_timeControl);
            }
        }

        /// <summary>
        /// Duration of the animation
        /// </summary>
        public float Duration 
        { 
            get => _duration;
            set
            {
                _duration = value;
                if (_timer == null)
                    _timer = new TimeKeeper(1 / _duration, _timeControl);
                else
                    _timer.Speed = 1 / _duration;
            }
        }

        /// <summary>
        /// Target value to animate towards to
        /// </summary>
        public T Target { get => _target; protected set => _target = value; }

        /// <summary>
        /// Updates the property value
        /// </summary>
        public void Update()
        {
            _setValue.Invoke(_timer.Tick());
        }

        /// <summary>
        /// Updates the property value syncronously
        /// </summary>
        /// <param name="time">The time given is in real time seconds</param>
        public void UpdateSync(float time)
        {

            if (time < _startTimeStamp)
                _setValue.Invoke(0);
            else if(time > _endTimeStamp)
                _setValue.Invoke(1);

            else if(time >= _startTimeStamp && time <= _endTimeStamp)
            {   
                // Since the real measured time is between the animateable time frame
                // it has to be offset by some value below the current deltaTime
                _timer.SetTime(time - _startTimeStamp);

                // And once this offset has been applied the next tick will always be one frame ahead
                // This value is always the current time delta hence we substrack it from the tick
                _setValue.Invoke(_timer.Tick() - Time.deltaTime);
            }
                
        }

        /// <summary>
        /// Creates an interpolator for the property
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="setValue"></param>
        public void CreateInterpolator(T startVal, Action<T> setValue)
        {
            var func = SetEasingFunction();
            _timer = new TimeKeeper(1 / _duration, _timeControl);
            _setValue = (t) =>
            {   
                var val = IncrimentValue(startVal, t, func);
                setValue(val);
                OnValueChanged?.Invoke(val);
                if (_timer.Time == 0)
                    OnStartReached?.Invoke(val);
                if (_timer.Time >= 1)
                {
                    OnTargetReached?.Invoke(val);
                    if(_timeControl == TimeControl.OneShot)
                        _animate = false;
                }
            };
        }

        /// <summary>
        /// Syncronizes the property animation with set max duration
        /// </summary>
        /// <param name="maxDuration"></param>
        public void Syncronize(float maxDuration)
        {
            Duration = maxDuration - (maxDuration * TrimFront) - (maxDuration * (1 - TrimBack));
            _endTimeStamp = maxDuration * _trimBack;
            _startTimeStamp = maxDuration - (maxDuration * (1 - _trimFront));
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
