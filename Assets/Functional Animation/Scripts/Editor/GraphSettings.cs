using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Aikom.FunctionalAnimation.Editor
{
    [FilePath("Assets/Functional Animation/UI/GraphSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GraphSettings : ScriptableSingleton<GraphSettings>
    {
        [SerializeField] private int _vertexCount;
        [SerializeField] private int _gridLineCount;
        [SerializeField] private List<Color> _legendColors = new();
        [SerializeField] private bool _snap;

        public int VertexCount { get { return _vertexCount; } internal set { _vertexCount = value; } }
        public int GridLineCount { get { return _gridLineCount; } internal set { _gridLineCount = value; } }
        public List<Color> LegendColors { get { return _legendColors; } internal set { _legendColors = value; } }
        public bool SnapToGrid { get { return _snap; } internal set { _snap = value; } }

        public GraphSettings GetOrLoadDefaults()
        {
            var instance = GraphSettings.instance;
            if(instance.VertexCount == 0)
            {
                instance.VertexCount = 500;
                instance.GridLineCount = 10;
                instance.LegendColors = new List<Color> 
                { 
                    Color.red,
                    Color.green,
                    Color.blue,
                    Color.white,
                };
                instance.SnapToGrid = true;
            }
            return instance;
        }

        public void SaveValues()
        {
            Save(true);
        }
    }
}

