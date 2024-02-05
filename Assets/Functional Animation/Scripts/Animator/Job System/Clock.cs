using Unity.Burst;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct Clock
    {
        private float _time;
        private float _speed;
        private int _direction;
        private TimeControl _timeControl;

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

        public Clock(float speed, TimeControl ctrl = TimeControl.OneShot)
        {
            if (speed == float.PositiveInfinity)
                speed = 0;
            _speed = speed;
            _direction = 1;
            _time = 0;
            _timeControl = ctrl;
        }

        /// <summary>
        /// Ticks the time by a custom delta and returns the current time
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public float Tick(float delta)
        {
            return _timeControl switch
            {
                TimeControl.Loop => Loop(delta),
                TimeControl.PingPong => PingPong(delta),
                TimeControl.OneShot => Forward(delta),
                _ => _time,
            };
        }

        /// <summary>
        /// Resets the time to 0
        /// </summary>
        public void Reset() => _time = 0;

        private float Forward(float delta)
        {
            _time += _speed * delta * _direction;
            _time = Mathf.Clamp01(_time);
            return _time;
        }

        private float PingPong(float delta)
        {
            if (_time >= 1)
                _direction = -1;
            else if (_time <= 0)
                _direction = 1;
            return Forward(delta);
        }

        private float Loop(float delta)
        {
            if (_time >= 1)
                _time = 0;
            return Forward(delta);
        }
    }
}

