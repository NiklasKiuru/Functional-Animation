using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class ScriptableTransformAnimator : MonoBehaviour
    {
        [SerializeReference] private TransformAnimation[] _animations = new TransformAnimation[0];
        [SerializeReference] private bool _playOnAwake;
        [SerializeReference] private string _playAwakeName;

        private Dictionary<int, TransformAnimation> _atlas = new Dictionary<int, TransformAnimation>();
        private EventContainer[] _eventContainer;
        private RuntimeController _runtimeController = new();


        /// <summary>
        /// Loads an animation from an animation object
        /// </summary>
        /// <param name="anim"></param>
        internal void Load(TransformAnimation anim)
        {
            _runtimeController.SetAnimation(anim, transform);
        }

        /// <summary>
        /// Play a preassigned animation clip
        /// </summary>
        /// <param name="name"></param>
        public void Play(string name)
        {
            Play(name.GetHashCode());
        }

        /// <summary>
        /// Play a preassigned animation clip
        /// </summary>
        /// <param name="hash"></param>
        public void Play(int hash)
        {
            if(_atlas.TryGetValue(hash, out var anim))
            {
                SetBinding((i, c) => _eventContainer[i].UnBind(c));
                Load(anim);
                SetBinding((i, c) => _eventContainer[i].Bind(c));

                void SetBinding(Action<int, Interpolator<Vector3>> bind)
                {
                    for(int i = 0; i < 3; i++)
                    {
                        if (_runtimeController.VectorInterpolators[i] == null)
                            continue;
                        bind(i, _runtimeController.VectorInterpolators[i]);
                    }
                }
            }    
        }

        /// <summary>
        /// Add a new animation into the animator
        /// </summary>
        /// <param name="anim"></param>
        public void AddAnimation(TransformAnimation anim, bool play = false)
        {
            if(anim == null)
                return;
            var hash = anim.name.GetHashCode();
            if(!_atlas.ContainsKey(hash))
                _atlas.Add(hash, anim);
            if(play)
                Play(hash);
        }

        private void Update()
        {
            _runtimeController.Update();
        }

        private void Awake()
        {   
            InitializeEventContainers();
            if (_playOnAwake && !string.IsNullOrEmpty(_playAwakeName))
                Play(_playAwakeName);
        }

        protected void OnValidate()
        {   
            _atlas.Clear();
            if(_eventContainer == null)
                InitializeEventContainers();

            for (int i = 0; i < _animations.Length; i++)
            {
                if (_animations[i] == null)
                    continue;
                _atlas.Add(_animations[i].name.GetHashCode(), _animations[i]);
            }
        }

        private void RegisterCallBack(int propContainerIndex, Action<Vector3> callback, EventType evt)
        {
            if(propContainerIndex < _eventContainer.Length && propContainerIndex > 0)
            {
                switch(evt)
                {
                    case EventType.OnValueChanged:
                        _eventContainer[propContainerIndex].OnValueChanged += callback;
                        break;
                    case EventType.OnTargetReached:
                        _eventContainer[propContainerIndex].OnTargetReached += callback;
                        break;
                    case EventType.OnStartReached:
                        _eventContainer[propContainerIndex].OnStartReached += callback;
                        break;
                }
            }
        }

        private void UnRegisterCallBack(int propContainerIndex, Action<Vector3> callback, EventType evt)
        {
            if(propContainerIndex < _eventContainer.Length && propContainerIndex > 0)
            {
                switch(evt)
                {
                    case EventType.OnValueChanged:
                        _eventContainer[propContainerIndex].OnValueChanged -= callback;
                        break;
                    case EventType.OnTargetReached:
                        _eventContainer[propContainerIndex].OnTargetReached -= callback;
                        break;
                    case EventType.OnStartReached:
                        _eventContainer[propContainerIndex].OnStartReached -= callback;
                        break;
                }
            }
        }

        public void RegisterCallBack(TransformProperty prop, Action<Vector3> cb, EventType evt) => RegisterCallBack((int)prop, cb, evt);

        public void UnRegisterCallBack(TransformProperty prop, Action<Vector3> cb, EventType evt) => UnRegisterCallBack((int)prop, cb, evt);

        private void InitializeEventContainers()
        {
            _eventContainer = new EventContainer[3];
            for (int i = 0; i < _eventContainer.Length; i++)
            {
                _eventContainer[i] = new EventContainer();
            }
        }

        /// <summary>
        /// Separate container for animation events. This has to exist due to the ability to switch animations in runtime
        /// Without this there can be possible memory leaks
        /// </summary>
        private class EventContainer
        {
            public Action<Vector3> OnStartReached;
            public Action<Vector3> OnTargetReached;
            public Action<Vector3> OnValueChanged;

            /// <summary>
            /// Binds animation events to the container
            /// </summary>
            /// <param name="cont"></param>
            public void Bind(Interpolator<Vector3> cont)
            {   
                cont.OnStartReached += OnStartReached;
                cont.OnTargetReached += OnTargetReached;
                cont.OnValueChanged += OnValueChanged;
            }

            /// <summary>
            /// Unbinds animation events from the container
            /// </summary>
            /// <param name="cont"></param>
            public void UnBind(Interpolator<Vector3> cont)
            {
                cont.OnStartReached += OnStartReached;
                cont.OnTargetReached += OnTargetReached;
                cont.OnValueChanged += OnValueChanged;
            }
        }
    }
}

