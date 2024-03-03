using UnityEditor;
using UnityEngine;

namespace Aikom.FunctionalAnimation.Editor
{
    [CustomPropertyDrawer(typeof(GraphData))]
    public class GraphAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GraphData data;
            if(property.serializedObject.targetObject is ScriptableObject)
            {
                // Do something else. The GetValue<T> does not work for ScriptableObjects
                return;
            }
            else
                data = property.GetValue<GraphData>();

            if (data == null || data?.Length == 0)
            {
                data = new GraphData();
                property.SetValue(data);
            }
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            Rect buttonRect = new Rect(position.x, position.y, 60, position.height);
            if(GUI.Button(buttonRect, "Edit"))
            {
                var window = EditorWindow.GetWindow<GraphEditorWindow>();
                window.titleContent = new GUIContent("Graph Editor");
                window.Show();
                window.SetData(data, property);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}

