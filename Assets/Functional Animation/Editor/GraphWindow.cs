using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using System.Linq;

namespace Aikom.FunctionalAnimation.Editor
{
    /// <summary>
    /// This script shows the animator graph in the editor
    /// The UI building here is messy at best since I did not want to spend too much time on it
    /// </summary>
    internal class GraphWindow : EditorWindow
    {
        private GraphRenderElement _renderElement;
        private Action _unregisterCbs;
        private TextField[] _normalTimeValues = new TextField[3];
        private TextField[] _accelerationValues = new TextField[3];
        private VisualElement _scriptableElement;

        [MenuItem("Window/Functional Animation")]
        public static void Init()
        {
            var window = GetWindow<GraphWindow>();
            window.titleContent = new GUIContent("Graph Window");
        }

        private void OnEnable()
        {   
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/GraphWindowStyles.uss");
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // Render element
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI/GraphMaterial.mat");
            _renderElement = new GraphRenderElement(material);
            _renderElement.style.width = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.height = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            _renderElement.style.borderLeftWidth = new StyleFloat(2);
            _renderElement.style.borderBottomWidth = new StyleFloat(2);
            _renderElement.style.borderLeftColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            _renderElement.style.borderBottomColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));

            // Side bar
            var sideBar = new VisualElement();
            sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            sideBar.style.flexDirection = FlexDirection.Column;

            var header = new Label("Properties");
            header.style.fontSize = 13;
            header.style.unityTextAlign = TextAnchor.MiddleCenter;
            header.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));
            header.style.marginTop = new StyleLength(new Length(5f, LengthUnit.Pixel));
            
            // Graph settings
            var subHeaderGraphSettings = new Label("Graph settings");
            subHeaderGraphSettings.style.fontSize = 12;
            subHeaderGraphSettings.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));

            var graphVertexCountSlider = new SliderInt("Vertex count")
            {
                highValue = 1000,
                lowValue = 100,
                value = _renderElement.SampleAmount,
                showInputField = true
            };

            var gridLineAmountSlider = new SliderInt("Grid line count")
            {
                highValue = GraphRenderElement.MaxGridLines,
                lowValue = 5,
                value = _renderElement.GridLines,
                showInputField = true
            };

            // Animator properties
            var animatorHeader = new Label("Animator");
            animatorHeader.style.fontSize = 12;
            animatorHeader.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));
            animatorHeader.style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));

            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
            var dummyContainer = new VisualElement();
            dummyContainer.style.minWidth = new StyleLength(new Length(153f, LengthUnit.Pixel));
            headerContainer.Add(dummyContainer);

            var headerLabelContainer = new VisualElement();
            headerLabelContainer.style.flexDirection = FlexDirection.Row;
            headerLabelContainer.style.flexGrow = 1;
            headerContainer.Add(headerLabelContainer);

            var timeHeader = new Label("Time");
            timeHeader.style.fontSize = 12;
            timeHeader.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));
            timeHeader.style.marginTop = new StyleLength(new Length(5f, LengthUnit.Pixel));
            timeHeader.style.unityTextAlign = TextAnchor.MiddleLeft;
            timeHeader.style.width = new StyleLength(new Length(50f, LengthUnit.Percent));
            timeHeader.style.marginLeft = new StyleLength(new Length(3f, LengthUnit.Pixel));
            timeHeader.style.marginRight = new StyleLength(new Length(3f, LengthUnit.Pixel));

            var accelerationHeader = new Label("Acceleration");
            accelerationHeader.style.unityTextAlign = TextAnchor.MiddleLeft;
            accelerationHeader.style.fontSize = 12;
            accelerationHeader.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));
            accelerationHeader.style.marginTop = new StyleLength(new Length(5f, LengthUnit.Pixel));
            accelerationHeader.style.width = new StyleLength(new Length(50f, LengthUnit.Percent));
            accelerationHeader.style.marginLeft = new StyleLength(new Length(3f, LengthUnit.Pixel));
            accelerationHeader.style.marginRight = new StyleLength(new Length(3f, LengthUnit.Pixel));

            headerLabelContainer.Add(timeHeader);
            headerLabelContainer.Add(accelerationHeader);

            var positionInfoContainer = CreatePropertyFields("Position", 0);
            var rotationInfoContainer = CreatePropertyFields("Rotation", 1);
            var scaleInfoContainer = CreatePropertyFields("Scale", 2);

            // Custom element for scriptable animator
            _scriptableElement = new VisualElement();
            _scriptableElement.style.flexDirection = FlexDirection.Column;
            _scriptableElement.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));

            var scriptableHeader = new Label("Scriptable Animator");
            scriptableHeader.style.fontSize = 12;
            scriptableHeader.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));
            scriptableHeader.style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));
            scriptableHeader.style.unityTextAlign = TextAnchor.MiddleLeft;

            var animationNameField = new TextField{ label = "Animation name" };
            var button = new Button { text = "Load" };

            _scriptableElement.Add(scriptableHeader);
            _scriptableElement.Add(animationNameField);
            _scriptableElement.Add(button);
            _scriptableElement.style.visibility = Visibility.Hidden;

            // Tree construction
            sideBar.Add(header);
            sideBar.Add(subHeaderGraphSettings);
            sideBar.Add(graphVertexCountSlider);
            sideBar.Add(gridLineAmountSlider);
            sideBar.Add(animatorHeader);
            sideBar.Add(headerContainer);
            sideBar.Add(positionInfoContainer);
            sideBar.Add(rotationInfoContainer);
            sideBar.Add(scaleInfoContainer);
            sideBar.Add(_scriptableElement);
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
                UnBindLoadButton();
            }

            VisualElement CreatePropertyFields(string label, int index)
            {
                var infoContainer = new VisualElement();
                infoContainer.AddToClassList("unity-base-field");
                infoContainer.style.flexDirection = FlexDirection.Row;
                infoContainer.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));

                var propertyLabel = new Label(label);
                propertyLabel.AddToClassList("unity-float-field__label");
                propertyLabel.style.minWidth = new StyleLength(new Length(150f, LengthUnit.Pixel));
                

                var dummy = new VisualElement();
                dummy.style.flexGrow = 1;
                dummy.style.flexDirection = FlexDirection.Row;
                dummy.style.marginRight = new StyleLength(new Length(2f, LengthUnit.Pixel));

                var time = new TextField();
                time.style.width = new StyleLength(new Length(50f, LengthUnit.Percent));

                var acceleration = new TextField();
                acceleration.style.width = new StyleLength(new Length(50f, LengthUnit.Percent));

                dummy.Add(time);
                dummy.Add(acceleration);
                infoContainer.Add(propertyLabel);
                infoContainer.Add(dummy);

                _accelerationValues[index] = acceleration;
                _normalTimeValues[index] = time;
                return infoContainer;
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
                if(anim is ScriptableTransformAnimator)
                {
                    _scriptableElement.style.visibility = Visibility.Visible;
                    _scriptableElement.Q<Button>().clicked += LoadAnimation;
                }
            }
            else
            {
                _renderElement.Animator = null;
                UnBindLoadButton();
            }
        }

        private void UnBindLoadButton()
        {
            _scriptableElement.style.visibility = Visibility.Hidden;
            _scriptableElement.Q<Button>().clicked -= LoadAnimation;
        }

        private void LoadAnimation()
        {
            var name = _scriptableElement.Q<TextField>().value;
            var scriptableAnimator = _renderElement.Animator as ScriptableTransformAnimator;
            if (scriptableAnimator != null || !string.IsNullOrEmpty(name))
                scriptableAnimator.Play(name);
        }

        private void OnInspectorUpdate()
        {
            if(_renderElement.Animator != null && !Application.isPlaying)
                Repaint();
            else if(_renderElement.Animator != null && Application.isPlaying)
            {
                for (int i = 0; i < 3; i++)
                {
                    var container = _renderElement.Animator.Container[i];
                    if (container != null)
                    {   
                        var time = container.Time;
                        _normalTimeValues[i].value = time.ToString("F2");
                        _accelerationValues[i].value = (MathUtils.Derivate(container.EasingFunc, 
                            time, _renderElement.MeasurementInterval) * container.Direction).ToString("F2");
                    }

                }
            }
                
        }

        private void Update()
        {
            if (Application.isPlaying && _renderElement.Animator != null)
            {   
                // This is for playhead update mainly
                _renderElement.DrawGraph();
                Repaint();
            }   
        }

        /// <summary>
        /// Visual element that draws the animation graph
        /// Drawing is currently done by using GL calls and I might update this in the future to use the UIElements Mesh API instead for more flexibility
        /// </summary>
        private class GraphRenderElement : ImmediateModeElement
        {
            private const int c_maxGridLines = 20;

            private Material _graphMaterial;
            private int _sampleAmount = 200;
            private int _gridLines = 10;
            private Label[] _gridMarkers = new Label[c_maxGridLines];
            private Label[] _gridTimeMarkers = new Label[c_maxGridLines];
            private int _currentMarkers = 0;
            private float _measurementInterval;

            public static int MaxGridLines { get => c_maxGridLines; }
            public TransformAnimator Animator { get; set; }
            public int SampleAmount { get => _sampleAmount; set => _sampleAmount = value; }
            public int GridLines { get => _gridLines; set => _gridLines = value; }
            public float MeasurementInterval { get => _measurementInterval; }

            public GraphRenderElement(Material lineMaterial)
            {
                _graphMaterial = lineMaterial;
                for(int i = 0; i < c_maxGridLines; i++)
                {
                    Add(_gridMarkers[i] = CreateLabel());
                    Add(_gridTimeMarkers[i] = CreateLabel());
                }

                static Label CreateLabel()
                {
                    var label = new Label();
                    label.style.unityTextAlign = TextAnchor.MiddleCenter;
                    label.style.visibility = Visibility.Hidden;
                    label.style.flexGrow = 0;
                    label.style.width = new StyleLength(new Length(20f, LengthUnit.Pixel));
                    label.style.height = new StyleLength(new Length(20f, LengthUnit.Pixel));
                    label.style.position = Position.Absolute;
                    return label;
                }

                var mainContainer = new VisualElement();
                mainContainer.style.marginTop = new StyleLength(new Length(30f, LengthUnit.Pixel));
                mainContainer.style.marginLeft = new StyleLength(new Length(5f, LengthUnit.Pixel));
                Add(mainContainer);

                CreateLegend("Postion", Color.red);
                CreateLegend("Rotation", Color.blue);
                CreateLegend("Scale", Color.green);

                void CreateLegend(string name, Color legendColor)
                {
                    var legContainer = new VisualElement();
                    legContainer.style.flexDirection = FlexDirection.Row;
                    legContainer.style.marginBottom = new StyleLength(new Length(2f, LengthUnit.Pixel));
                    legContainer.style.marginTop = new StyleLength(new Length(2f, LengthUnit.Pixel));
                    legContainer.style.alignItems = Align.Center;

                    var colorContainer = new VisualElement();
                    colorContainer.style.backgroundColor = new StyleColor(legendColor);
                    colorContainer.style.width = new StyleLength(new Length(10f, LengthUnit.Pixel));
                    colorContainer.style.height = new StyleLength(new Length(10f, LengthUnit.Pixel));

                    var label = new Label(name);
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;
                    label.style.marginLeft = new StyleLength(new Length(3f, LengthUnit.Pixel));

                    legContainer.Add(colorContainer);
                    legContainer.Add(label);
                    mainContainer.Add(legContainer);
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
                
                // **TEST**
                //var methodInfo = Animator.GetType().GetMethods().Where(p => Attribute.IsDefined(p, typeof(GraphMethodAttribute))).FirstOrDefault();
                //var attr = methodInfo.GetCustomAttributes(typeof(GraphMethodAttribute), false).FirstOrDefault();
                //var name = (attr as GraphMethodAttribute).name;
                //methodInfo.Invoke(Animator, new object[] { name });
               
                // Begin draw call
                GL.PushMatrix();
                _graphMaterial.SetPass(0);
                GL.LoadOrtho();
                GL.Begin(GL.LINES);

                // Draws the grid and labels. The labels have really weird positioning logic that i mainly
                // just eyeballed to get them to look right. Should still look relatively good when
                // resizing the window
                GL.Color(new Color(1,1,1,0.2f));
                var interval = 1f / _gridLines;
                for(int j = 1; j <= _gridLines; j++)
                {
                    float x = j * interval;

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
                    _measurementInterval = sampleInc;

                    // Unsyncronized time control
                    if (!Animator.SyncAll)
                    {
                        for (int sample = 0; sample < _sampleAmount; sample++)
                        {
                            DrawVertexSingle(sample);
                            DrawVertexSingle(sample + 1);
                        }
                        currentPositions[i] = GetAbsolutePos(container.Time, func(container.Time));
                    }
                    // Syncronized time control. The time variables in the window are
                    // not precise at all times but marker positions should be
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

