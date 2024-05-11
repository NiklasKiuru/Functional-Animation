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
        private int _currentLoop;
        private int _maxLoopCount;
        private bool _isCompleted;

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

        /// <summary>
        /// Max number of loops allowed for this clock
        /// </summary>
        public int MaxLoops { get => _maxLoopCount; set => _maxLoopCount = value; }

        /// <summary>
        /// Current loop of the clock
        /// </summary>
        public int CurrentLoop { get => _currentLoop; }

        /// <summary>
        /// Flag for completion status
        /// </summary>
        public bool IsCompleted { get => _isCompleted; }

        public Clock(float speed, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (speed == float.PositiveInfinity)
                speed = 0;
            _speed = speed;
            _direction = 1;
            _time = 0;
            _timeControl = ctrl;
            _currentLoop = 0;
            _maxLoopCount = -1;
            _isCompleted = false;
        }

        /// <summary>
        /// Ticks the time by a custom delta and returns the current time
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public float Tick(float delta)
        {
            _time += _speed * delta * _direction;
            return _timeControl switch
            {
                TimeControl.Loop => Loop(),
                TimeControl.PingPong => PingPong(),
                TimeControl.PlayOnce => Forward(),
                _ => _time,
            };
        }

        /// <summary>
        /// Inverts the direction of time
        /// </summary>
        public void InvertDirection() => _direction *= -1;

        /// <summary>
        /// Resets the clock
        /// </summary>
        public void Reset()
        {
            _time = 0;
            _direction = 1;
            _currentLoop = 0;
        }

        private float Forward()
        {   
            _time = Mathf.Clamp01(_time);
            _isCompleted = CheckCompletion();
            return _time;
        }

        private float PingPong()
        {   
            if (_time >= 1)
            {
                _direction = -1;
                _time = 1 - Mathf.Abs(1 - _time);
                _currentLoop++;
                _isCompleted = CheckCompletion();
                if (_isCompleted)
                    _time = 1;
            }
                
            else if (_time <= 0)
            {
                _direction = 1;
                _time = Mathf.Abs(_time);
                _currentLoop++;
                _isCompleted = CheckCompletion();
                if(_isCompleted)
                    _time = 0;
            }
                
            return _time;
        }

        private float Loop()
        {
            if (_time >= 1)
            {
                _time = 1 - _time;
                _currentLoop++;
                _isCompleted = CheckCompletion();
                if (_isCompleted)
                    _time = 1;
            }
            return _time;
        }

        /// <summary>
        /// Returns the completion status of this timer
        /// </summary>
        /// <returns></returns>
        private bool CheckCompletion()
        {
            return (_timeControl == TimeControl.PlayOnce && (_time >= 1 && _direction == 1 || _time == 0 && _direction == -1)) 
                || _maxLoopCount - _currentLoop == 0;
        }
    }
}

