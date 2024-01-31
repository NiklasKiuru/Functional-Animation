using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Aikom.FunctionalAnimation.Editor
{
    public class GraphEditorWindow : EditorWindow
    {

        public GraphData GraphData { get; set; }

        [MenuItem("Window/Functional Animation/Graph Editor")]
        public static void Init()
        {
            var window = GetWindow<GraphEditorWindow>();
            window.titleContent = new GUIContent("Graph Editor");
            window.Show();
        }

        private void CreateGUI()
        {
            Debug.Log(GraphData == null);
        }
    }
}

