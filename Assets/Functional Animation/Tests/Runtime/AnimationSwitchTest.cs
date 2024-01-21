using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Aikom.FunctionalAnimation.Tests
{
    internal class SwitchTestWindow : EditorWindow
    {
        private ScriptableTransformAnimator _animator;
        private TransformAnimation _animation;

        [MenuItem("Examples/My Editor Window")]
        public static void Init()
        {
            var window = GetWindow<SwitchTestWindow>();
        }

        private void OnGUI()
        {
            _animator = FindFirstObjectByType<ScriptableTransformAnimator>();
            _animation = EditorGUILayout.ObjectField("Animation", _animation, typeof(TransformAnimation), false) as TransformAnimation;
            if (GUILayout.Button("Switch"))
            {   
                if(_animator != null && _animation != null)
                {
                    _animator.Load(_animation);
                }
            }
        }

    }
}

