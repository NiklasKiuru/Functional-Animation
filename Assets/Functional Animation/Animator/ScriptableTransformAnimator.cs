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

        private Dictionary<int, RuntimeContainer> _atlas = new Dictionary<int, RuntimeContainer>();
        private RuntimeController _runtimeController = new();
        private RuntimeContainer _currentAnimation;

        public RuntimeController RuntimeController { get => _runtimeController; }


        private void Load(RuntimeContainer cont)
        {
            SetBinding((i, c) => cont.EventContainer[i].UnBind(c));
            _runtimeController.SetAnimation(cont.Animation, transform);
            SetBinding((i, c) => cont.EventContainer[i].Bind(c));

            void SetBinding(Action<int, Interpolator<Vector3>> bind)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (_runtimeController.VectorInterpolators[i] == null)
                        continue;
                    bind(i, _runtimeController.VectorInterpolators[i]);
                }
            }
            _currentAnimation = cont;
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
                Load(anim);
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
                _atlas.Add(hash, new RuntimeContainer(anim));
            if(play)
                Play(hash);
        }

        /// <summary>
        /// Resets the currently playing animation
        /// </summary>
        public void ResetAnimation()
        {
            if (_currentAnimation != null)
                _runtimeController.ResetCurrent();
        }

        /// <summary>
        /// Resets current animations property into its initial values
        /// </summary>
        /// <param name="prop"></param>
        public void ResetProperty(TransformProperty prop)
        {
            if(_currentAnimation != null)
                _runtimeController.ResetProperty(prop);
        }

        private void Update()
        {
            _runtimeController.Update();
        }

        private void Awake()
        {   
            _atlas.Clear();
            for (int i = 0; i < _animations.Length; i++)
            {
                if (_animations[i] == null)
                    continue;
                _atlas.Add(_animations[i].name.GetHashCode(), new RuntimeContainer( _animations[i]));
            }

            if (_playOnAwake && !string.IsNullOrEmpty(_playAwakeName))
                Play(_playAwakeName);
        }


        public void RegisterCallBack(TransformProperty prop, Action<Vector3> cb, EventType evt, int animationId)
        {
            if(!_atlas.TryGetValue(animationId, out var cont))
                return;
            switch(evt)
            {
                case EventType.OnStartReached:
                    cont.EventContainer[(int)prop].OnStartReached += cb;
                    break;
                case EventType.OnTargetReached:
                    cont.EventContainer[(int)prop].OnTargetReached += cb;
                    break;
                case EventType.OnValueChanged:
                    cont.EventContainer[(int)prop].OnValueChanged += cb;
                    break;
            }
        }

        public void UnRegisterCallBack(TransformProperty prop, Action<Vector3> cb, EventType evt, int animationId)
        {
            if (!_atlas.TryGetValue(animationId, out var cont))
                return;
            switch (evt)
            {
                case EventType.OnStartReached:
                    cont.EventContainer[(int)prop].OnStartReached -= cb;
                    break;
                case EventType.OnTargetReached:
                    cont.EventContainer[(int)prop].OnTargetReached -= cb;
                    break;
                case EventType.OnValueChanged:
                    cont.EventContainer[(int)prop].OnValueChanged -= cb;
                    break;
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

        private class RuntimeContainer
        {
            public TransformAnimation Animation;
            public EventContainer[] EventContainer;

            public RuntimeContainer(TransformAnimation anim)
            {
                Animation = anim;
                EventContainer = new EventContainer[3] 
                { 
                    new EventContainer(), 
                    new EventContainer(), 
                    new EventContainer() 
                };
            }
        }
    }
}

