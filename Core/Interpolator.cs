using System;
using UnityEngine;
using System.Collections;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Interpolator class that can store and control values independently
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Interpolator<T> where T : struct, IEquatable<T>
    {   
        public enum Status { Running, Stopped, Paused }

        // Private fields
        private T _target;
        private T _default;
        private bool _hasActiveRoutine;
        private T _currentValue;
        private TimeKeeper _timer;

        //private static RoutineController _controllerInstance;

        // Events
        public event Action<T> OnValueChanged;
        public event Action<T> OnTargetReached;
        public event Action<T> OnStartReached;
        public event Action<T> OnRoutineFinished;
        public event Action<T> OnRoutineStarted;

        // Cached functions and actions
        private Func<float, T> _mainFunction;
        private Func<float, T, T, T> _subFunction;
        private Action<T> _setVal;

        public float Speed { get => _timer.Speed; }
        public T Target { get => _target; }
        public T Default { get => _default; }
        public T CurrentValue { get => _currentValue; }
        public float LinearTime { get => _timer.Time; }
        public int Direction { get { return _timer.Direction; } }
        public Func<float, T> Main { get => _mainFunction; }
        internal TimeKeeper Timer { get => _timer; }
        public Status InternalState { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interpolation">Handles the interpolation logic. Parameters in order: Time, Start, End</param>
        /// <param name="setVal">Optional automatic value set action</param>
        /// <param name="speed">Interpolation speed</param>
        /// <param name="target"></param>
        /// <param name="ctrl"></param>
        public Interpolator(Func<float, T, T, T> interpolation, Action<T> setVal, float speed, T start, T target, TimeControl ctrl = TimeControl.OneShot)
        {
            _setVal = setVal;
            _target = target;
            _default = start;
            _subFunction = interpolation;

            _timer = new TimeKeeper(speed, ctrl);
            _mainFunction = (t) =>
            {
                var val = _subFunction(t, _default, _target);
                if (!_currentValue.Equals(val))
                    OnValueChanged?.Invoke(val);
                _currentValue = val;
                _setVal?.Invoke(val);
                if (_timer.Time <= 0)
                    OnStartReached?.Invoke(val);
                if (_timer.Time >= 1)
                    OnTargetReached?.Invoke(val);
                return val;
            };

            if(ctrl == TimeControl.OneShot)
                OnTargetReached += SetStatus;
        }

        ~Interpolator()
        {
            OnTargetReached -= SetStatus;
        }

        private void SetStatus(T val)
        {
            if(_timer.Time == 1)
                InternalState = Status.Stopped;
        }

        /// <summary>
        /// Overrides currently set set value action
        /// </summary>
        /// <param name="setVal"></param>
        public void SetValueOverride(Action<T> setVal)
        {
            if (setVal != null && !_hasActiveRoutine)
                _setVal = setVal;
        }

        /// <summary>
        /// Overrides current start and target values. Values cannot be overrdden while a routine is running
        /// Resets the internal clock to 0
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void OverrideValues(T start, T end)
        {   
            if(_hasActiveRoutine)
                return;
            _default = start;
            _target = end;
            _timer.Reset();
        }

        /// <summary>
        /// Overrides the current target value. Value cannot be overrdden while a routine is running
        /// </summary>
        /// <param name="newTarget"></param>
        /// <param name="restart"></param>
        public void OverrideTarget(T newTarget, bool restart = true)
        {   
            if(_hasActiveRoutine)
                return;
            _target = newTarget;
            if (restart)
                _timer.Reset();
        }

        /// <summary>
        /// Runs the interpolation with set parameters in delta time
        /// </summary>
        public void Run() => _mainFunction(_timer.Tick());

        /// <summary>
        /// Runs the interpolation with set parameters in custom delta time
        /// </summary>
        /// <param name="delta"></param>
        public void Run(float delta) => _mainFunction(_timer.Tick(delta));

        /// <summary>
        /// Runs the interpolation with set parameters in delta time + offset
        /// </summary>
        /// <param name="offset"></param>
        public void RunOffset(float offset) => _mainFunction(_timer.Tick() + offset);

        /// <summary>
        /// Samples the interpolation result value at a given linear time point between 0 and 1
        /// This never invokes set value action or events and never sets the current value and can be used while a routine is running
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public T Sample(float time) => _subFunction(time, _default, _target);

        /// <summary>
        /// Stops the current running routine if there is one
        /// </summary>
        public void StopRoutine()
        {
            _hasActiveRoutine = false;
            OnRoutineFinished?.Invoke(_currentValue);
        }

        /// <summary>
        /// Inverts the direction of time in the interpolator. Can only be used while no routine is running
        /// </summary>
        public void InvertDirection()
        {
            if (!_hasActiveRoutine)
                _timer.InvertDirection();
        }

        /// <summary>
        /// Resets the interpolator to its default value. Stops the current routine if there is one
        /// </summary>
        public void Reset()
        {
            _timer.Reset();
            _mainFunction(0);
            if(_hasActiveRoutine)
                StopRoutine();
            InternalState = Status.Running;
        }

        /// <summary>
        /// Starts a coroutine to pingpong the value once
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="turningCondition">If no turning condition is given the default condition is met once the timer end position has been reached</param>
        public void PingPongRoutine(MonoBehaviour owner, Func<bool> turningCondition = null)
        {
            if(_timer.Direction == -1)
                _timer.InvertDirection();
            turningCondition ??= () => _timer.Time >= 1;
            
            owner.StartCoroutine(Run(OnYield, terminate));

            void terminate(T s)
            {
                if (_timer.Time == 0)
                    StopRoutine();
            }
            void OnYield() { if (turningCondition()) _timer.InvertDirection(); }
        }

        /// <summary>
        /// Runs the interpolation once and terminates. Cannot be used while a routine is already running
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="customTerminationCondition">Terminates the routine prematurely if set</param>
        public void StartRoutine(MonoBehaviour owner, Func<bool> customTerminationCondition = null)
        {
            if (_hasActiveRoutine)
                return;
            bool DefaultCondition() => (_timer.Time >= 1 && _timer.Direction == 1) || (_timer.Time <= 0 && _timer.Direction == -1);
            customTerminationCondition ??= DefaultCondition;
            void Terminate(T s) 
            {
                if (customTerminationCondition())
                {
                    StopRoutine();
                    _timer.Reset();
                }  
            };
            owner.StartCoroutine(Run(null, Terminate));
        }

        private IEnumerator Run(Action onYield, Action<T> terminate)
        {   
            OnRoutineStarted?.Invoke(_currentValue);
            _hasActiveRoutine = true;
            while(_hasActiveRoutine)
            {
                Run();
                onYield?.Invoke();
                terminate.Invoke(_currentValue);
                yield return null;
            }
        }

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
    }
}
