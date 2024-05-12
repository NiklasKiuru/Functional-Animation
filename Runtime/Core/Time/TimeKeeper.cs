using System;
using UnityEngine;


namespace Aikom.FunctionalAnimation
{
    public enum TimeControl
    {
        Loop,
        PingPong,
        PlayOnce
    }

    /// <summary>
    /// Timer object that holds information about linear clamped time value
    /// </summary>
    public class TimeKeeper
    {
        private float _time;
        private float _speed;
        private int _direction;
        private TimeControl _timeControl;
        private Func<float, float> _controller;

        /// <summary>
        /// Current linear time [0, 1]
        /// </summary>
        public float Time { get => _time; }

        /// <summary>
        /// Current real time in seconds
        /// </summary>
        public float RealTime { get => _time / _speed; }

        /// <summary>
        /// Duration of the time cycle the keeper measures
        /// </summary>
        public float Duration { get => 1 / _speed; }

        /// <summary>
        /// Speed of the linear time scale
        /// </summary>
        public float Speed { get => _speed; set => _speed = value; }

        /// <summary>
        /// Contoller type
        /// </summary>
        public TimeControl TimeControl { get => _timeControl; }

        /// <summary>
        /// Current direction of time
        /// </summary>
        public int Direction { get => _direction; }

        public event Action OnLoopCompleted;

        public TimeKeeper(float speed, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (speed == float.PositiveInfinity)
                speed = 0;
            _speed = speed;
            _direction = 1;
            SetTimeControl(ctrl);
        }

        /// <summary>
        /// Sets the control function
        /// </summary>
        /// <param name="timeControl"></param>
        public void SetTimeControl(TimeControl timeControl)
        {
            _timeControl = timeControl;
            switch (_timeControl)
            {
                case TimeControl.Loop:
                    _controller = Loop;
                    break;
                case TimeControl.PingPong:
                    _controller = PingPong;
                    break;
                case TimeControl.PlayOnce:
                    _controller = Forward;
                    break;
            }
        }

        /// <summary>
        /// Ticks the time by Unitys Time.deltaTime and returns the current time
        /// </summary>
        /// <returns></returns>
        public float Tick() => _controller.Invoke(UnityEngine.Time.deltaTime);

        /// <summary>
        /// Ticks the time by a custom delta and returns the current time
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public float Tick(float delta) => _controller.Invoke(delta);

        /// <summary>
        /// Sets the time as in real time seconds
        /// </summary>
        /// <param name="time"></param>
        public void SetTime(float time)
        {   
            _time = time / Duration;
            _time = Mathf.Clamp01(_time);
        }

        /// <summary>
        /// Resets the time to 0
        /// </summary>
        public void Reset() => _time = 0;

        /// <summary>
        /// Inverts the direction of time
        /// </summary>
        public void InvertDirection() => _direction *= -1;

        /// <summary>
        /// Pauses the timer
        /// </summary>
        public void Pause() => _controller = (d) => { return _time; };

        /// <summary>
        /// Continues the timer if the timer was paused
        /// </summary>
        public void Continue() => SetTimeControl(_timeControl);

        private float Forward(float delta)
        {
            _time += _speed * delta * _direction;
            _time = Mathf.Clamp01(_time);
            if(_time == 1 || _time == 0)
                OnLoopCompleted?.Invoke();
            return _time;
        }

        private float PingPong(float delta)
        {
            _time += _speed * delta * _direction;
            if (_time >= 1)
            {
                _direction = -1;
                _time = 1 - Mathf.Abs(1 - _time);
                OnLoopCompleted?.Invoke();
            }

            else if (_time <= 0)
            {
                _direction = 1;
                _time = Mathf.Abs(_time);
                OnLoopCompleted?.Invoke();
            }

            return _time;
        }

        private float Loop(float delta)
        {
            _time += _speed * delta * _direction;
            if (_time >= 1)
            {
                _time = Mathf.Abs(1 - _time);
                OnLoopCompleted?.Invoke();
            }
                
            return _time;
        }
    }
}

