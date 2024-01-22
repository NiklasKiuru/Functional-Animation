using UnityEngine;
using UnityEditor;

namespace Aikom.FunctionalAnimation.Editor
{
    [CustomEditor(typeof(ScriptableTransformAnimator))]
    public class ScriptableAnimatorEditor : UnityEditor.Editor
    {   
        private SerializedProperty _animations;
        private SerializedProperty _playOnAwake;
        private SerializedProperty _playAwakeName;

        private void OnEnable()
        {
            _animations = serializedObject.FindProperty("_animations");
            _playOnAwake = serializedObject.FindProperty("_playOnAwake");
            _playAwakeName = serializedObject.FindProperty("_playAwakeName");
        }

        public override void OnInspectorGUI()
        {   
            serializedObject.Update();
            EditorGUILayout.PropertyField(_playOnAwake);
            if(_playOnAwake.boolValue)
                EditorGUILayout.PropertyField(_playAwakeName);
            _animations.isExpanded = EditorGUILayout.Foldout(_animations.isExpanded, "Animations");
            if (_animations.isExpanded)
            {
                EditorGUI.indentLevel++;

                // The field for item count
                _animations.arraySize = EditorGUILayout.IntField("Size", _animations.arraySize, GUILayout.Width(230));
                if(_animations.arraySize < 0)
                    _animations.arraySize = 0;
                // draw item fields
                for (var i = 0; i < _animations.arraySize; i++)
                {
                    var item = _animations.GetArrayElementAtIndex(i);
                    string name;
                    if (item.objectReferenceValue == null)
                        name = $"Animation {i}";
                    else
                        name = item.objectReferenceValue.name;
                    EditorGUILayout.PropertyField(item, new GUIContent($"{name}"));
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
