using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    /// <summary>
    /// This script shows the animator graph in the editor
    /// The UI building here is messy at best since I did not want to spend too much time on it
    /// </summary>
    internal class GraphWindow : EditorWindow
    {
        private RenderElement _renderElement;
        private Action _unregisterCbs;
        private FloatField[] _normalTimeValues;

        [MenuItem("Examples/My Editor Window")]
        public static void Init()
        {
            var window = GetWindow<GraphWindow>();
            window.titleContent = new GUIContent("Graph Window");
        }

        private void OnEnable()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GraphMaterial.mat");
            _renderElement = new RenderElement(material, root);
            _renderElement.style.width = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.height = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            _renderElement.style.borderLeftWidth = new StyleFloat(2);
            _renderElement.style.borderBottomWidth = new StyleFloat(2);
            _renderElement.style.borderLeftColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            _renderElement.style.borderBottomColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));

            var sideBar = new VisualElement();
            sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            sideBar.style.flexDirection = FlexDirection.Column;

            var header = new Label("Properties");
            header.style.fontSize = 13;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));
            header.style.marginTop = new StyleLength(new Length(5f, LengthUnit.Pixel));
            
            var subHeaderGraphSettings = new Label("Graph settings");
            subHeaderGraphSettings.style.fontSize = 12;
            subHeaderGraphSettings.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));

            var graphVertexCountSlider = new SliderInt("Vertex count");
            graphVertexCountSlider.highValue = 1000;
            graphVertexCountSlider.lowValue = 100;
            graphVertexCountSlider.value = _renderElement.SampleAmount;
            graphVertexCountSlider.showInputField = true;
            
            var gridLineAmountSlider = new SliderInt("Grid line count");
            gridLineAmountSlider.highValue = RenderElement.MaxGridLines;
            gridLineAmountSlider.lowValue = 5;
            gridLineAmountSlider.value = _renderElement.GridLines;
            gridLineAmountSlider.showInputField = true;

            var animatorHeader = new Label("Animator");
            animatorHeader.style.fontSize = 12;
            animatorHeader.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));
            animatorHeader.style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));

            var positionValue = new FloatField();
            positionValue.label = "Position";

            var rotationValue = new FloatField();
            rotationValue.label = "Rotation";

            var scaleValue = new FloatField();
            scaleValue.label = "Scale";

            _normalTimeValues = new FloatField[3] { positionValue, rotationValue, scaleValue };

            sideBar.Add(header);
            sideBar.Add(subHeaderGraphSettings);
            sideBar.Add(graphVertexCountSlider);
            sideBar.Add(gridLineAmountSlider);
            sideBar.Add(animatorHeader);
            sideBar.Add(positionValue);
            sideBar.Add(rotationValue);
            sideBar.Add(scaleValue);

            root.Add(sideBar);
            root.Add(_renderElement);

            // Register callbacks and set a fallback method for unbinds
            Selection.selectionChanged += SetAnimatior;
            RegisterCallbacks();
            _unregisterCbs = UnRegisterCallbacks;

            void ChangeVertexCount(ChangeEvent<int> e) => _renderElement.SampleAmount = e.newValue;
            void ChangeGridLineCount(ChangeEvent<int> e) => _renderElement.GridLines = e.newValue;

            void RegisterCallbacks()
            {
                graphVertexCountSlider.RegisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.RegisterValueChangedCallback(ChangeGridLineCount);
            }

            void UnRegisterCallbacks()
            {
                graphVertexCountSlider.UnregisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.UnregisterValueChangedCallback(ChangeGridLineCount);
            }
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= SetAnimatior;
            _unregisterCbs?.Invoke();
        }

        private void SetAnimatior()
        {
            var obj = Selection.activeGameObject;
            if (obj != null && obj.TryGetComponent<TransformAnimator>(out var anim))
            {
                _renderElement.Animator = anim;
            }
                
            else
                _renderElement.Animator = null;
        }

        private void OnInspectorUpdate()
        {
            if(_renderElement.Animator != null && !Application.isPlaying)
                Repaint();
        }

        private void Update()
        {
            if (Application.isPlaying && _renderElement.Animator != null)
            {   
                for(int i = 0; i < 3; i++)
                {
                    if (_renderElement.Animator.Container[i] != null)
                        _normalTimeValues[i].value = _renderElement.Animator.Container[i].Timer.Time;
                }
                _renderElement.DrawGraph();
                Repaint();
            }   
        }

        /// <summary>
        /// Visual element that draws the animation graph
        /// Drawing is currently done by using GL calls and I might update this in the future to use the UIElements Mesh API instead for more flexibility
        /// </summary>
        private class RenderElement : ImmediateModeElement
        {
            private const int c_maxGridLines = 20;

            private Material _graphMaterial;
            private int _sampleAmount = 200;
            private int _gridLines = 10;
            private Label[] _gridMarkers = new Label[c_maxGridLines];
            private Label[] _gridTimeMarkers = new Label[c_maxGridLines];
            private int _currentMarkers = 0;

            public static int MaxGridLines { get => c_maxGridLines; }
            public TransformAnimator Animator { get; set; }
            public int SampleAmount { get => _sampleAmount; set => _sampleAmount = value; }
            public int GridLines { get => _gridLines; set => _gridLines = value; }

            public RenderElement(Material lineMaterial, VisualElement root)
            {
                _graphMaterial = lineMaterial;
                for(int i = 0; i < c_maxGridLines; i++)
                {
                    _gridMarkers[i] = new Label();
                    _gridMarkers[i].style.unityTextAlign = TextAnchor.MiddleCenter;
                    _gridMarkers[i].style.visibility = Visibility.Hidden;
                    _gridMarkers[i].style.flexGrow = 0;
                    _gridMarkers[i].style.width = new StyleLength(new Length(20f, LengthUnit.Pixel));
                    _gridMarkers[i].style.height = new StyleLength(new Length(20f, LengthUnit.Pixel));
                    _gridMarkers[i].style.position = Position.Absolute;
                    _gridTimeMarkers[i] = new Label();
                    _gridTimeMarkers[i].style.unityTextAlign = TextAnchor.MiddleCenter;
                    _gridTimeMarkers[i].style.visibility = Visibility.Hidden;
                    _gridTimeMarkers[i].style.flexGrow = 0;
                    _gridTimeMarkers[i].style.width = new StyleLength(new Length(20f, LengthUnit.Pixel));
                    _gridTimeMarkers[i].style.height = new StyleLength(new Length(20f, LengthUnit.Pixel));
                    _gridTimeMarkers[i].style.position = Position.Absolute;

                    Add(_gridMarkers[i]);
                    Add(_gridTimeMarkers[i]);
                }
            }

            protected override void ImmediateRepaint()
            {
                DrawGraph();
            }

            public void DrawGraph()
            {
                if (Animator == null)
                    return;
                
                // Begin draw call
                GL.PushMatrix();
                _graphMaterial.SetPass(0);
                GL.LoadOrtho();
                GL.Begin(GL.LINES);

                // Draws the grid
                GL.Color(new Color(1,1,1,0.2f));
                for(int j = 1; j <= _gridLines; j++)
                {
                    float x = j * (1f / _gridLines);

                    // X-axis
                    DrawVertex(x, 0);
                    DrawVertex(x, 1);
                    var timeMarker = _gridTimeMarkers[j - 1];
                    timeMarker.style.visibility = Visibility.Visible;
                    var duration = Animator.SyncAll? Animator.MaxDuration : 1f;
                    timeMarker.text = (x * duration).ToString("F2");

                    timeMarker.style.left = x * layout.width * 0.935f - (timeMarker.layout.width / 2);
                    timeMarker.style.top = timeMarker.layout.height / 2;

                    // Y-axis
                    DrawVertex(0, x);
                    DrawVertex(1, x);
                    var gridMarker = _gridMarkers[j - 1];
                    gridMarker.style.visibility = Visibility.Visible;
                    gridMarker.text = (x).ToString("F2");
                    gridMarker.style.left = layout.width - gridMarker.layout.width * 2.5f;
                    gridMarker.style.bottom = x * layout.height * 0.9f - (gridMarker.layout.height / 2);
                }

                if(_gridLines > _currentMarkers)
                    _currentMarkers = _gridLines;
                else
                    DisableInactiveMarkers();

                var currentPositions = new Vector3[3];

                for (int i = 0; i < 3; i++)
                {
                    SetColor(i);
                    var container = Animator.Container[i];
                    if (container == null || !container.Animate)
                        continue;
                    var func = container.FunctionConstructor.Generate();
                    float sampleInc = 1f / _sampleAmount;

                    // Unsyncronized time control
                    if (!Animator.SyncAll)
                    {
                        for (int sample = 0; sample < _sampleAmount; sample++)
                        {
                            DrawVertexSingle(sample);
                            DrawVertexSingle(sample + 1);
                        }
                        currentPositions[i] = GetAbsolutePos(container.Timer.Time, func(container.Timer.Time));
                    }
                    // Syncronized time control
                    else
                    {
                        float duration = container.TrimBack - container.TrimFront;
                        for (int sample = 0; sample < _sampleAmount; sample++)
                        {   
                            DrawVertexTrimmed(sample * sampleInc);
                            DrawVertexTrimmed((sample + 1) * sampleInc);
                        }

                        currentPositions[i] = GetAbsolutePos(Animator.Timer.Time, TrimValues(Animator.Timer.Time).y);

                        void DrawVertexTrimmed(float x)
                        {
                            var vec = TrimValues(x);
                            DrawVertex(vec.x, vec.y);
                        }

                        Vector2 TrimValues(float x)
                        {
                            float y;
                            if (x <= container.TrimFront)
                                y = func.Invoke(0);
                            else if (x >= container.TrimBack)
                                y = func.Invoke(1);
                            else
                                y = func.Invoke((x - container.TrimFront) / duration);
                            return new Vector2(x, y);
                        }
                    }

                    Vector3 DrawVertexSingle(int index)
                    {
                        float x = index * sampleInc;
                        return DrawVertex(x, func(x));
                    }
                }

                if(Application.isPlaying)
                {
                    // Draws the playhead
                    GL.Color(Color.white);
                    DrawVertex(Animator.Timer.Time, 0);
                    DrawVertex(Animator.Timer.Time, 1);
                    GL.End();

                    // Draws the current time markers
                    GL.Begin(GL.QUADS);
                    var sideLen = 0.01f;
                    for (int i = 0; i < 3; i++)
                    {
                        if (Animator.Container[i] == null || !Animator.Container[i].Animate)
                            continue;
                        SetColor(i);
                        GL.Vertex3(currentPositions[i].x - sideLen, currentPositions[i].y - sideLen, 0);
                        GL.Vertex3(currentPositions[i].x - sideLen, currentPositions[i].y + sideLen, 0);
                        GL.Vertex3(currentPositions[i].x + sideLen, currentPositions[i].y + sideLen, 0);
                        GL.Vertex3(currentPositions[i].x + sideLen, currentPositions[i].y - sideLen, 0);
                    }
                }
                GL.End();
                GL.PopMatrix();
            }

            private void SetColor(int index)
            {
                switch (index)
                {
                    case 0:
                        GL.Color(Color.red);    // Position
                        break;
                    case 1:
                        GL.Color(Color.blue);   // Rotation
                        break;
                    case 2:
                        GL.Color(Color.green);  // Scale
                        break;
                }
            }

            private void DisableInactiveMarkers()
            {
                for(int i = _gridLines; i < _currentMarkers; i++)
                {
                    _gridMarkers[i].style.visibility = Visibility.Hidden;
                    _gridTimeMarkers[i].style.visibility = Visibility.Hidden;
                }
                _currentMarkers = _gridLines;
            }

            /// <summary>
            /// Draws a vertex on the graph panel and returns the absolute position of the vertex
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public Vector3 DrawVertex(float x, float y)
            {   
                var pos = GetAbsolutePos(x, y);
                GL.Vertex3(pos.x, pos.y, pos.z);
                return pos;
            }

            /// <summary>
            /// Calculates the absolute position of a point on the graph panel
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public Vector3 GetAbsolutePos(float x, float y) => new Vector3((x * 0.75f) + 0.2f, (y * 0.7f) + 0.2f, 0);

        }

    }
}

