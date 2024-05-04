using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Aikom.FunctionalAnimation.Editor
{
    [CustomPropertyDrawer(typeof(FunctionAlias))]
    public sealed class FunctionAliasDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var options = BurstFunctionCache.GetDefinitions().Select((a) => a.Value).ToArray();
            var val = property.FindPropertyRelative("_value");
            var hash = property.FindPropertyRelative("_hash");
            var current = 0;
            for(int i = 0; i < options.Length; i++)
            {
                if (options[i] == val.stringValue)
                {
                    current = i;
                    break;
                }
            }
            var index = EditorGUI.Popup(position, label.text, current, options);
            val.stringValue = options[index];
            hash.longValue = FunctionAlias.GetHash(val.stringValue);    // Has to be long due to int32 overflow
        }
    }
}
