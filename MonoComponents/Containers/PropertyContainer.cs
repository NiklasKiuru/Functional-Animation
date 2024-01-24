using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public abstract class PropertyContainer<T> where T : struct, IEquatable<T>
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

        // Sycronization variables
        private float _startTimeStamp;
        private float _endTimeStamp;

        // Main interpolator
        private Interpolator<T> _interpolator;

        // Cached easing function
        protected Func<float, float> _easingFunc;

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

        /// <summary>
        /// The proportion of the animation that is not played from the beginning.
        /// If played syncronously the full graph will be played in renormalized time relative to animators clock
        /// </summary>
        public float TrimFront { get => _trimFront; protected set => _trimFront = value; }

        /// <summary>
        /// The proportion of the animation that is not played from the end.
        /// If played syncronously the full graph will be played in renormalized time relative to animators clock
        /// </summary>
        public float TrimBack { get => _trimBack; protected set => _trimBack = value; }

        /// <summary>
        /// Current linear time [0, 1]
        /// </summary>
        public float Time 
        {
            get
            {
                if (_interpolator == null)
                    return 0;
                else
                    return _interpolator.Timer.Time;
            }
        }

        /// <summary>
        /// Time control for the property animation
        /// </summary>
        public TimeControl TimeControl 
        { 
            get => _timeControl;
            set
            {
                _timeControl = value;
                _interpolator?.Timer.SetTimeControl(value);
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
                if (_interpolator != null)
                    _interpolator.Timer.Speed = 1 / _duration;
            }
        }

        /// <summary>
        /// Target value to animate towards to
        /// </summary>
        public T Target { get => _target; protected set => _target = value; }
        public Func<float, float> EasingFunc { get => _easingFunc; }
        public int Direction 
        { 
            get 
            {
                if(_interpolator == null)
                    return 1;
                return _interpolator.Direction; 
            } 
        }

        public PropertyContainer()
        {
            _easingFunc = GenerateEasingFunction();
        }

        public PropertyContainer(T start, T target, float duration, Action<T> setVal, TimeControl timeControl = TimeControl.OneShot)
        {
            _target = target;
            _duration = duration;
            _timeControl = timeControl;
            _easingFunc = GenerateEasingFunction();
            CreateInterpolator(start, setVal);
        }

        /// <summary>
        /// Updates the property value
        /// </summary>
        public void Update()
        {
            _interpolator.Run();
        }

        /// <summary>
        /// Updates the property value syncronously in relation to animators clock
        /// </summary>
        /// <param name="time">The time given is in real time seconds</param>
        public void UpdateSync(float time)
        {
            if (time < _startTimeStamp)
                _interpolator.RunOffset(-1);
            else if (time > _endTimeStamp)
                _interpolator.RunOffset(1);
            else if(time >= _startTimeStamp && time <= _endTimeStamp)
            {   
                _interpolator.Timer.SetTime(time - _startTimeStamp);
                _interpolator.RunOffset(-UnityEngine.Time.deltaTime);
            }
        }

        /// <summary>
        /// Creates an interpolator for the property
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="setValue"></param>
        public void CreateInterpolator(T startVal, Action<T> setValue)
        {   
            // Disable event hooks
            if(_interpolator != null)
            {
                _interpolator.OnStartReached -= OnStartReached;
                _interpolator.OnTargetReached -= OnTargetReached;
                _interpolator.OnValueChanged -= OnValueChanged;
            }
            OnInitialize(startVal);
            _easingFunc = GenerateEasingFunction();
            _interpolator = new Interpolator<T>(IncrimentValue, setValue, 1 / _duration, startVal, _target,  _timeControl);

            // Rebind event hooks
            _interpolator.OnStartReached += OnStartReached;
            _interpolator.OnTargetReached += OnTargetReached;
            _interpolator.OnValueChanged += OnValueChanged;
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
        /// The method defining how the property value is changed
        /// </summary>
        /// <param name="startVal"></param>
        /// <param name="time"></param>
        /// <param name="easingFunc"></param>
        /// <returns></returns>
        internal abstract T IncrimentValue(float time, T start, T end);

        /// <summary>
        /// Caches the easing function
        /// </summary>
        /// <returns></returns>
        protected abstract Func<float, float> GenerateEasingFunction();

        protected virtual void OnInitialize(T start)
        {

        }
    }

    public enum EventType
    {
        OnValueChanged,
        OnTargetReached,
        OnStartReached
    }
}
