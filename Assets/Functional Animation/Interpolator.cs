using System;
using UnityEngine;
using System.Collections;

namespace Aikom.FunctionalAnimation
{
    public class Interpolator<T> where T : struct
    {   
        /// <summary>
        /// Interpolation target. Should be used as a static target but can also be changed dynamically
        /// </summary>
        private T _target;

        private T _default;

        /// <summary>
        /// Main function used in interpolation
        /// </summary>
        private Func<float, T> _mainFunction;

        private Action<T> _setVal;

        /// <summary>
        /// Flag that defines if the interpolator has an active routine looping
        /// </summary>
        private bool _hasActiveRoutine;

        /// <summary>
        /// Current interpolation value
        /// </summary>
        private T _currentValue;

        private TimeKeeper _timer;

        private static RoutineController _controllerInstance;

        public event Action<T> OnValueChanged;
        public event Action<T> OnTargetReached;
        public event Action<T> OnStartReached;

        public float Speed { get => _timer.Speed; set => _timer.Speed = value; }
        public T Target { get => _target; set => _target = value; }
        public T Default { get => _default; set => _default = value; }
        public T CurrentValue { get => _currentValue; }
        public float LinearTime { get => _timer.Time; }
        public float Direction
        {
            get { return _timer.Direction; }
        }

        public Func<float, T> Main { get => _mainFunction; }

        /// <summary>
        /// Constructor used to create Interpolator if generated from a script
        /// </summary>
        /// <param name="mainFunction"></param>
        /// <param name="setVal"></param>
        /// <param name="speed"></param>
        /// <param name="target"></param>
        /// <param name="ctrl"></param>
        public Interpolator(Func<float,T, T, T> mainFunction, Action<T> setVal, float speed, T target, T defaultValue, TimeControl ctrl = TimeControl.OneShot)
        {
            _setVal = setVal;
            _target = target;
            _default = defaultValue;

            _timer = new TimeKeeper(1 / speed, ctrl);
            Action<float> main = (t) => 
            {   
                var val = mainFunction(t, defaultValue, target);
                _setVal(val);
                if(!_currentValue.Equals(val))
                    OnValueChanged?.Invoke(val);
                if (_timer.Time <= 0)
                    OnStartReached?.Invoke(val);
                if (_timer.Time >= 1)
                    OnTargetReached?.Invoke(val);
            };
        }

        /// <summary>
        /// Overrides current main function. Override is only possible if no routines are running and given function cannot be null
        /// </summary>
        /// <param name="mainFunction"></param>
        public void OverrideFunction(Func<float, T> mainFunction)
        {   
            if(mainFunction != null && !_hasActiveRoutine)
                _mainFunction = mainFunction;
        }

        ///// <summary>
        ///// Modulates the value with set parameters with constructed functionality
        ///// This function should be called once every frame and will not get invoked if a routine is running
        ///// </summary>
        ///// <param name="value"></param>
        //public float Animate(float value)
        //{
        //    if (!_hasActiveRoutine)
        //    {
        //        value = AnimateInternal();
        //    }
        //    return value;
        //}

        ///// <summary>
        ///// Modulates the value with set parameters with constructed functionality
        ///// </summary>
        ///// <returns></returns>
        //public float Animate()
        //{
        //    return AnimateInternal();
        //}

        ///// <summary>
        ///// Samples the animation at a given linear time point between 0 and 1
        ///// </summary>
        ///// <param name="time"></param>
        ///// <returns></returns>
        //public float Sample(float time)
        //{
        //    return EF.Interpolate(_mainFunction, _default, _target, time);
        //}

        ///// <summary>
        ///// Starts a coroutine animation to pingpong the animation once. Very useful for zoom in - out effects
        ///// </summary>
        ///// <param name="owner"></param>
        ///// <param name="onYield"></param>
        ///// <param name="turningCondition"></param>
        //public void PingPongRountine(MonoBehaviour owner, Action<float> onYield, Func<bool> turningCondition)
        //{
        //    _direction = 1;
        //    Action<float> terminate = (s) =>
        //    {
        //        if (s == _default)
        //            StopRoutine();
        //    };

        //    if (_hasActiveRoutine)
        //        StopRoutine();
        //    owner.StartCoroutine(_activeRoutine.Numerator);
        //}

        ///// <summary>
        ///// Inverts the time direction of the animation
        ///// </summary>
        ///// <param name="invert">If true interpolates from target towards origin</param>
        //public void InvertDirection(bool invert)
        //{
        //    _direction = invert ? -1 : 1;
        //}

        ///// <summary>
        ///// Inverts the time direction of the animation
        ///// </summary>
        //public void InvertDirection()
        //{
        //    _direction *= -1;
        //}

        ///// <summary>
        ///// Starts a coroutine animation
        ///// </summary>
        ///// <param name="owner"></param>
        ///// <param name="value"></param>
        ///// <param name="terminateOnCompletion">Whether the rountine should be automatically terminated upon completion</param>
        //public void StartRoutine(MonoBehaviour owner, Action<float> onYield, bool terminateOnCompletion = true)
        //{
        //    if(!_hasActiveRoutine)
        //        _time = _direction == -1 ? 1 : 0;
        //    Action terminate = null;
        //    if (terminateOnCompletion)
        //    {
        //        terminate = () =>
        //        {
        //            if ((_time >= 1 && _direction == 1) || (_time <= 0 && _direction == -1))
        //                StopRoutine();
        //        };
        //    }
            
        //    if(_hasActiveRoutine)
        //        StopRoutine();
        //    owner.StartCoroutine(Animate(terminate, onYield));
        //}

        ///// <summary>
        ///// Stops the current routine
        ///// </summary>
        //public void StopRoutine()
        //{
        //    _hasActiveRoutine = false;
        //    OnRoutineFinished?.Invoke();
        //}

        ///// <summary>
        ///// Starts a static instance routine
        ///// </summary>
        ///// <param name="onYield"></param>
        ///// <param name="terminateOnCompletion"></param>
        //public void StartStaticRoutine(Action<float> onYield, bool terminateOnCompletion = true)
        //{
        //    if(_controllerInstance == null)
        //    {
        //        var obj = new GameObject("RoutineController");
        //        var controller = obj.AddComponent<RoutineController>();
        //        _controllerInstance = controller;
        //    }

        //    StartRoutine(_controllerInstance, onYield, terminateOnCompletion);
        //}

        //private IEnumerator Animate(Action onCompletion, Action<float> onYield)
        //{
        //    _hasActiveRoutine = true;
        //    while (_hasActiveRoutine)
        //    {   
        //        float value = AnimateInternal();
        //        onYield?.Invoke(value);
        //        //Debug.Log(value);
        //        onCompletion?.Invoke();
        //        yield return null;
        //    }
        //}

        //private IEnumerator AnimatePingPong(Action<float> onCompletion, Action<float> onYield, Func<bool> turningCondition)
        //{
        //    _hasActiveRoutine = true;
        //    while (_hasActiveRoutine)
        //    {
        //        float value = AnimateInternal();
        //        onYield?.Invoke(value);
        //        if (turningCondition())
        //            _direction = -1;
        //        onCompletion.Invoke(value);
        //        yield return null;
        //    }
        //}


        //private float AnimateInternal()
        //{
        //    _timer.Tick();
        //    _currentValue = EF.Interpolate(_mainFunction, _default, _target, _time);
        //    return _currentValue;
        //}
    }
}
