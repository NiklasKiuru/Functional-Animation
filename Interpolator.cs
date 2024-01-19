using System;
using UnityEngine;
using System.Collections;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class Interpolator : ISerializationCallbackReceiver
    {   
        /// <summary>
        /// Constructor that defines used easing functions. Overrides main function
        /// </summary>
        [Tooltip("Constructor used to determine interpolation functions")]
        [SerializeField] private FunctionConstructor _constructor;

        /// <summary>
        /// Speed multiplies the normalized time parameter to adjust the animation time to any positive scale
        /// </summary>
        [Tooltip("How fast should the animation be played")]
        [SerializeField] private float _speed;

        /// <summary>
        /// Interpolation target. Should be used as a static target but can also be changed dynamically
        /// </summary>
        [Tooltip("Target value to interpolate towards")]
        [SerializeField] private float _target;

        [Tooltip("Default value to interpolate from")]
        [SerializeField] private float _default;

        /// <summary>
        /// Main function used in interpolation
        /// </summary>
        private Func<float, float> _mainFunction;

        /// <summary>
        /// Normalized time parameter [0, 1]
        /// </summary>
        private float _time;

        /// <summary>
        /// Playback direction. Forward = 1, Backwards = -1
        /// </summary>
        private int _direction = 1;

        /// <summary>
        /// Flag that defines if the interpolator has an active routine looping
        /// </summary>
        private bool _loopRoutine;

        /// <summary>
        /// Active routine wrapper with owner component
        /// </summary>
        private Routine _activeRoutine;

        /// <summary>
        /// Current interpolation value
        /// </summary>
        private float _currentValue;

        private static RoutineController _controllerInstance;

        public event Action OnRoutineFinished;

        public float Speed { get => _speed; set => _speed = value; }
        public float Target { get => _target; set => _target = value; }
        public float Default { get => _default; set => _default = value; }
        public float CurrentValue { get => _currentValue; }
        public float LinearTime { get => _time; }
        public float Direction
        {
            get { return _direction; }
            set
            {
                {
                    if (value > 0)
                        _direction = 1;
                    else
                        _direction = -1;
                }
            }
        }

        public Func<float, float> Main { get => _mainFunction; }
        
        /// <summary>
        /// Constructor used to create Interpolator if generated from a script
        /// </summary>
        /// <param name="mainFunction"></param>
        /// <param name="speed"></param>
        /// <param name="target"></param>
        public Interpolator(Func<float, float> mainFunction, float speed, float target, float defaultValue)
        {
            _mainFunction = mainFunction;
            _speed = speed;
            _target = target;
            _default = defaultValue;
            _direction = 1;
        }

        /// <summary>
        /// Overrides current main function. Override is only possible if no routines are running and given function cannot be null
        /// </summary>
        /// <param name="mainFunction"></param>
        public void OverrideFunction(Func<float, float> mainFunction)
        {   
            if(mainFunction != null && !_loopRoutine && _activeRoutine == null)
                _mainFunction = mainFunction;
        }

        /// <summary>
        /// Modulates the value with set parameters with constructed functionality
        /// This function should be called once every frame and will not get invoked if a routine is running
        /// </summary>
        /// <param name="value"></param>
        public float Animate(float value)
        {
            if (!_loopRoutine)
            {
                value = AnimateInternal();
            }
            return value;
        }

        /// <summary>
        /// Modulates the value with set parameters with constructed functionality
        /// </summary>
        /// <returns></returns>
        public float Animate()
        {
            return AnimateInternal();
        }

        /// <summary>
        /// Samples the animation at a given linear time point between 0 and 1
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Sample(float time)
        {
            return EF.Interpolate(_mainFunction, _default, _target, time);
        }

        /// <summary>
        /// Pingpongs the animation back and forth indefinetely
        /// </summary>
        /// <param name="value"></param>
        public float PingPong(ref float value)
        {
            if(!_loopRoutine)
            {
                if(_direction > 0 && _time >= 1)
                    _direction = -1;
                else if(_direction <= 0 && _time <= 0)
                    _direction = 1;

                value = AnimateInternal();
            }

            return value;
        }

        /// <summary>
        /// Pingpongs the animation back and forth indefinetely
        /// </summary>
        public float PingPong()
        {
            if (!_loopRoutine)
            {
                if (_direction > 0 && _time >= 1)
                    _direction = -1;
                else if (_direction <= 0 && _time <= 0)
                    _direction = 1;

                return AnimateInternal();
            }

            return _default;
        }

        /// <summary>
        /// Loops the animation indefinetely
        /// </summary>
        /// <returns></returns>
        public float Loop()
        {
            var val = AnimateInternal();
            if(_time >= 1)
                _time = 0;
            return val;
        }

        /// <summary>
        /// Starts a coroutine animation to pingpong the animation once. Very useful for zoom in - out effects
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="onYield"></param>
        /// <param name="turningCondition"></param>
        public void PingPongRountine(MonoBehaviour owner, Action<float> onYield, Func<bool> turningCondition)
        {
            _direction = 1;
            Action<float> terminate = (s) =>
            {
                if (s == _default)
                    StopRoutine();
            };

            if (_activeRoutine != null)
                StopRoutine();
            _activeRoutine = new Routine(owner, AnimatePingPong(terminate, onYield, turningCondition));
            owner.StartCoroutine(_activeRoutine.Numerator);
        }

        /// <summary>
        /// Inverts the time direction of the animation
        /// </summary>
        /// <param name="invert">If true interpolates from target towards origin</param>
        public void InvertDirection(bool invert)
        {
            _direction = invert ? -1 : 1;
        }

        /// <summary>
        /// Inverts the time direction of the animation
        /// </summary>
        public void InvertDirection()
        {
            _direction *= -1;
        }

        /// <summary>
        /// Starts a coroutine animation
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="value"></param>
        /// <param name="terminateOnCompletion">Whether the rountine should be automatically terminated upon completion</param>
        public void StartRoutine(MonoBehaviour owner, Action<float> onYield, bool terminateOnCompletion = true)
        {
            if(!_loopRoutine)
                _time = _direction == -1 ? 1 : 0;
            Action terminate = null;
            if (terminateOnCompletion)
            {
                terminate = () =>
                {
                    if ((_time >= 1 && _direction == 1) || (_time <= 0 && _direction == -1))
                        StopRoutine();
                };
            }
            
            if(_activeRoutine != null)
                StopRoutine();
            _activeRoutine = new Routine(owner, Animate(terminate, onYield));
            owner.StartCoroutine(Animate(terminate, onYield));
        }

        /// <summary>
        /// Stops the current routine
        /// </summary>
        public void StopRoutine()
        {
            _loopRoutine = false;
            _activeRoutine.Owner.StopCoroutine(_activeRoutine.Numerator);
            _activeRoutine = null;
            OnRoutineFinished?.Invoke();
        }

        /// <summary>
        /// Starts a static instance routine
        /// </summary>
        /// <param name="onYield"></param>
        /// <param name="terminateOnCompletion"></param>
        public void StartStaticRoutine(Action<float> onYield, bool terminateOnCompletion = true)
        {
            if(_controllerInstance == null)
            {
                var obj = new GameObject("RoutineController");
                var controller = obj.AddComponent<RoutineController>();
                _controllerInstance = controller;
            }

            StartRoutine(_controllerInstance, onYield, terminateOnCompletion);
        }

        private IEnumerator Animate(Action onCompletion, Action<float> onYield)
        {
            _loopRoutine = true;
            while (_loopRoutine)
            {   
                float value = AnimateInternal();
                onYield?.Invoke(value);
                //Debug.Log(value);
                onCompletion?.Invoke();
                yield return null;
            }
        }

        private IEnumerator AnimatePingPong(Action<float> onCompletion, Action<float> onYield, Func<bool> turningCondition)
        {
            _loopRoutine = true;
            while (_loopRoutine)
            {
                float value = AnimateInternal();
                onYield?.Invoke(value);
                if (turningCondition())
                    _direction = -1;
                onCompletion.Invoke(value);
                yield return null;
            }
        }


        private float AnimateInternal()
        {
            _time += Time.deltaTime * _speed * _direction;
            _time = Mathf.Clamp01(_time);
            _currentValue = EF.Interpolate(_mainFunction, _default, _target, _time);
            return _currentValue;
        }

        public void OnAfterDeserialize()
        {
            if (_constructor != null)
            {
                _mainFunction = _constructor.Generate();
            }
        }

        public void OnBeforeSerialize()
        {
            if(_constructor != null)
            {
                _mainFunction = _constructor.Generate();
            }
        }

        private class Routine
        {
            public IEnumerator Numerator;
            public MonoBehaviour Owner;

            public Routine(MonoBehaviour owner, IEnumerator numerator)
            {
                Numerator = numerator;
                Owner = owner;
            }
        }
    }
}
