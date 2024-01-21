using UnityEditor;
using UnityEngine;

namespace Aikom.FunctionalAnimation.Editor
{
    [CustomEditor(typeof(TransformAnimator))]
    public class TransformAnimatorEditor : UnityEditor.Editor
    {
        

        private PropertyOptions _propertyOptions;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _propertyOptions = (PropertyOptions)EditorGUILayout.EnumFlagsField("Save property animation", _propertyOptions);
            if(GUILayout.Button("Save"))
            {   
                var path = EditorUtility.SaveFilePanelInProject("Save animation", "New Animation", "asset", "Save animation");
                var animator = target as TransformAnimator;
                if (string.IsNullOrEmpty(path))
                    return;
                TransformAnimation.Save(animator, path, _propertyOptions);
            }

        }
    }
}

