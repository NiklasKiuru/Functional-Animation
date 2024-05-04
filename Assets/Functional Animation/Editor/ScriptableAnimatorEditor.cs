using UnityEditor;

namespace Aikom.FunctionalAnimation.Editor
{
    [CustomEditor(typeof(ScriptableTransformAnimator))]
    public class ScriptableAnimatorEditor : UnityEditor.Editor
    {   
        private SerializedProperty _playOnAwake;
        private SerializedProperty _playAwakeName;


        private void OnEnable()
        {
            _playOnAwake = serializedObject.FindProperty("_playOnAwake");
            _playAwakeName = serializedObject.FindProperty("_playAwakeName");
        }

        public override void OnInspectorGUI()
        {   
            serializedObject.Update();
            EditorGUILayout.PropertyField(_playOnAwake);
            if(_playOnAwake.boolValue)
                EditorGUILayout.PropertyField(_playAwakeName);

            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
