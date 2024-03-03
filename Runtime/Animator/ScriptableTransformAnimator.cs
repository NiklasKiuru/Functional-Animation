using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Expandable simple animator
    /// </summary>
    public class ScriptableTransformAnimator : MonoBehaviour, IAnimator
    {
        [HideInInspector][SerializeReference] private bool _playOnAwake;
        [HideInInspector][SerializeReference] private string _playAwakeName;
        [SerializeField] private TransformAnimation[] _animations = new TransformAnimation[1];

        private Dictionary<int, TransformAnimation> _atlas = new Dictionary<int, TransformAnimation>();
        protected TransformAnimation _currentAnimation;
        protected TransformHandle _controlHandle = new();

        TransformHandle IAnimator.Handle { get =>  _controlHandle; }

        private void Load(TransformAnimation anim)
        {
            this.SetAnimation(anim, transform, SetVal);
            void SetVal(float3 val, TransformProperty prop) => prop.SetValue(transform, val);
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
        /// Resets the currently playing animation
        /// </summary>
        public void ResetCurrent()
        {
            _controlHandle.KillAll();
        }

        private void Awake()
        {   
            _atlas.Clear();
            for (int i = 0; i < _animations.Length; i++)
            {
                if (_animations[i] == null)
                    continue;
                _atlas.Add(_animations[i].name.GetHashCode(), _animations[i]);
            }

            if (_playOnAwake && !string.IsNullOrEmpty(_playAwakeName))
                Play(_playAwakeName);
        }
    }
}

