using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MathUtils = Aikom.FunctionalAnimation.Utility.MathUtils;

namespace Aikom.FunctionalAnimation.Editor
{
    /// <summary>
    /// Visual element that draws the animation graph
    /// Drawing is currently done by using GL calls and I might update this in the future to use the UIElements Mesh API instead for more flexibility
    /// </summary>
    public class GraphRenderElement : ImmediateModeElement
    {
        private const int c_maxGridLines = 20;
        private const float c_offsetX = 0.23f;
        private const float c_offsetY = 0.1f;
        private const float c_width = 0.73f;
        private const float c_height = 0.8f;

        private Vector2 _cachedSize;
        private LegendElement _legendContainer;
        private Material _graphMaterial;
        private int _sampleAmount = 1000;
        private int _gridLines = 10;
        private Label[] _gridMarkers = new Label[2 * c_maxGridLines + 1];
        private Label[] _gridTimeMarkers = new Label[c_maxGridLines + 1];
        private float _measurementInterval;
        private NodeElement[] _positionMarkers = new NodeElement[30];
        private NodeElement _activeDragElement;
        private VisualElement _root;
        private bool _isDragging = false;
        private float _xMult = 1;
        private float _yMult = 1;
        [SerializeField] private List<Color> _legendColors = new List<Color>();


        /// <summary>
        /// Fired when the render interface modifies the graph values
        /// </summary>
        public event Action OnGraphModified;

        public IList LegendColors { get { return _legendColors; } }
        public float YAxisMultiplier { get => _yMult; set => _yMult = Mathf.Max(value, 1); }
        public float XAxisMultiplier { get => _xMult; set => _xMult = Mathf.Max(value, 1); }
        /// <summary>
        /// Maximum allowed grid lines
        /// </summary>
        public static int MaxGridLines { get => c_maxGridLines; }

        /// <summary>
        /// Amount of times each graph function is sampled
        /// </summary>
        public int SampleAmount { get => _sampleAmount; set => _sampleAmount = value; }

        /// <summary>
        /// Currently drawn gridlines
        /// </summary>
        public int GridLines { get => _gridLines; set => _gridLines = value; }

        /// <summary>
        /// Time interval between each sample
        /// </summary>
        public float MeasurementInterval { get => _measurementInterval; }

        /// <summary>
        /// Data to draw
        /// </summary>
        public GraphData[] GraphData { get; private set; }

        /// <summary>
        /// Defines the index of the default Graph that should be used as the modification target
        /// </summary>
        public int DefaultModificationTarget { get; private set; }

        /// <summary>
        /// Whether to use grid snapping or not
        /// </summary>
        public bool SnapGrid { get; set; }
        private float DockAreaHeight { get => _root.parent.layout.height - _root.layout.height; }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="root"></param>
        public GraphRenderElement(VisualElement root, string legendHeader, string[] legendElements, GraphSettings settings)
        {
            _graphMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Functional Animation//Editor/Assets/GraphMaterial.mat");
            _root = root;
            style.width = new StyleLength(new Length(80f, LengthUnit.Percent));
            style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            style.borderLeftWidth = new StyleFloat(2);
            style.borderLeftColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            _sampleAmount = settings.VertexCount;
            _gridLines = settings.GridLineCount;
            _legendColors = settings.LegendColors;
            SnapGrid = settings.SnapToGrid;
            _cachedSize = new Vector2(resolvedStyle.width, resolvedStyle.height);

            // X-axis
            for (int i = 0; i <= c_maxGridLines; i++)
                Add(_gridTimeMarkers[i] = CreateLabel());
            // Y-Axis
            for(int i = 0; i <= 2 * c_maxGridLines; i++)
                Add(_gridMarkers[i] = CreateLabel());

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

            _legendContainer = new LegendElement(legendHeader);
            Add(_legendContainer);
            SetLegend(legendElements);
            CreateNodeMarkers();
            RegisterCallback<MouseMoveEvent>(OnMarkerElementPositionChanged);
            RegisterCallback<MouseUpEvent>(OnDragEnd);
        }

        // Destructor
        ~GraphRenderElement()
        {
            for (int i = 0; i < _positionMarkers.Length; i++)
            {
                _positionMarkers[i].UnregisterCallback<MouseDownEvent>(OnDragStart);
                _positionMarkers[i].UnregisterCallback<MouseUpEvent>(OnDragEnd);
            }
            UnregisterCallback<MouseMoveEvent>(OnMarkerElementPositionChanged);
            UnregisterCallback<MouseUpEvent>(OnDragEnd);
        }

        protected override void ImmediateRepaint()
        {
            DrawGraph();
        }

        #region Public Methods

        public void SetLegend(string[] names) => _legendContainer.SetAll(names, GetGraphColor);

        public void SetDrawTargets(int defaultModificationTarget, params GraphData[] data)
        {
            GraphData = data;
            DefaultModificationTarget = defaultModificationTarget;
            SetNodeMarkers();
            DrawGraph();
        }

        public void DisableDrawTarget(int index)
        {
            SetDrawTarget(index, null);
        }

        public void SetDrawTarget(int index, GraphData data)
        {
            if (GraphData == null || index < 0 || index >= GraphData?.Length)
                return;
            GraphData[index] = data;
            SetNodeMarkers();
        }

        public void SetModificationTarget(int target)
        {
            DefaultModificationTarget = target;
            DrawGraph();
            SetNodeMarkers();
        }

        public void SetLegendHeader(string header) => _legendContainer.Header = header;

        /// <summary>
        /// Sets and activates the node markers for the given axis
        /// </summary>
        /// <param name="axis"></param>
        public void SetNodeMarkers()
        {
            for (int i = 0; i < _positionMarkers.Length; i++)
                _positionMarkers[i].style.visibility = Visibility.Hidden;
            if (GraphData == null || GraphData.Length == 0)
                return;

            var activeMarkers = 0;
            for (int i = 0; i < GraphData.Length; i++)
            {
                if (GraphData[i] == null)
                    continue;
                var axisColor = GetGraphColor(i);
                var index = 0;
                var nodes = GraphData[i].Nodes;
                foreach (var node in nodes)
                {
                    var pos = GetAbsolutePos(node.x, node.y);
                    pos = new Vector2(pos.x * _root.layout.width, pos.y * (_root.layout.height + DockAreaHeight));
                    pos = this.WorldToLocal(pos);
                    var marker = _positionMarkers[activeMarkers];
                    marker.style.visibility = Visibility.Visible;
                    marker.style.left = pos.x - (marker.layout.width / 2) - 2;
                    marker.style.bottom = pos.y + (marker.layout.height / 2) + 10;
                    marker.Activate(index, axisColor, i, node);
                    activeMarkers++;
                    index++;
                }
            }
            
        }

        

        /// <summary>
        /// Main method for drawing the graph
        /// </summary>
        public void DrawGraph()
        {   
            // Guarantees that the markers are redrawn properly if the window is resized
            var size = new Vector2( resolvedStyle.width, resolvedStyle.height );
            if (size != _cachedSize)
                SetNodeMarkers();
            _cachedSize = size;

            if (GraphData == null || GraphData.Length == 0)
                return;

            // Begin draw call
            GL.PushMatrix();
            _graphMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.LINES);
            try
            {
                // Draws the grid and labels. The labels have really weird positioning logic that i mainly
                // just eyeballed to get them to look right. Should still look relatively good when
                // resizing the window
                GL.Color(new Color(1, 1, 1, 0.2f));
                DisableMarkers();

                // X-axis
                var interval = 1f / _gridLines;
                for (int j = 0; j <= _gridLines; j++)
                {
                    float x = j * interval;

                    
                    DrawVertex(x, -1);
                    var posX = DrawVertex(x, 1);
                    var timeMarker = _gridTimeMarkers[j];
                    timeMarker.style.visibility = Visibility.Visible;
                    timeMarker.text = (x * _xMult).ToString("F2");

                    posX = new Vector2(posX.x * _root.layout.width, posX.y * (_root.layout.height + DockAreaHeight));
                    posX = this.WorldToLocal(posX);
                    timeMarker.style.left = posX.x - (timeMarker.layout.width / 2);
                    timeMarker.style.top = timeMarker.layout.height / 2;
                }

                // Y-axis
                var index = 0;
                for(int j = -_gridLines; j <= _gridLines; j++)
                {
                    float y = j * interval;

                    if (y == 0)
                        GL.Color(new Color(1, 1, 1, 0.6f));
                    if (y == interval)
                        GL.Color(new Color(1, 1, 1, 0.2f));

                    DrawVertex(0, y);
                    var posY = DrawVertex(1, y);
                    var gridMarker = _gridMarkers[index];
                    gridMarker.style.visibility = Visibility.Visible;
                    gridMarker.text = (y).ToString("F2");

                    posY = new Vector2(posY.x * _root.layout.width, posY.y * (_root.layout.height + DockAreaHeight));
                    posY = this.WorldToLocal(posY);
                    gridMarker.style.left = layout.width - gridMarker.layout.width * 2.5f;
                    gridMarker.style.bottom = posY.y + (gridMarker.layout.height / 2);
                    index++;
                }

                _measurementInterval = 1f / _sampleAmount;
                for (int i = 0; i < GraphData.Length; i++)
                {
                    if (GraphData[i] == null)
                        continue;
                    SetColor(i);
                    var func = GraphData[i].GetEvaluator();
                    for (int sample = 0; sample < _sampleAmount; sample++)
                    {
                        DrawVertexSingle(sample);
                        DrawVertexSingle((sample + 1));
                    }

                    Vector3 DrawVertexSingle(int index)
                    {
                        float x = index * _measurementInterval;
                        return DrawVertex(x, func(x));
                    }
                }
            }
            catch (Exception e)
            {
                GL.End();
                Debug.Log(e.Message);
            }
            GL.End();
            GL.PopMatrix();
        }
        #endregion

        private void SetColor(int index)
        {
            GL.Color(GetGraphColor(index));
        }

        private Color GetGraphColor(int axis)
        {
            if(axis < _legendColors.Count)
                return _legendColors[axis];
            return Color.white;
        }

        private void DisableMarkers()
        {
            for(int i = 0; i < _gridMarkers.Length; i++)
            {
                if (i < _gridTimeMarkers.Length)
                    _gridTimeMarkers[i].style.visibility = Visibility.Hidden;
                _gridMarkers[i].style.visibility = Visibility.Hidden;
            }
        }

        private Vector3 DrawVertex(float x, float y)
        {
            var pos = GetAbsolutePos(x, y);
            GL.Vertex3(pos.x, pos.y, pos.z);
            return pos;
        }

        /// <summary>
        /// Gets the position on the graph from the given panel position
        /// </summary>
        /// <param name="panelPositon"></param>
        /// <returns></returns>
        public Vector2 GetGraphPosition(Vector2 panelPositon)
        {
            var graphPos = new Vector2(panelPositon.x / _root.layout.width, 1 - (panelPositon.y / (_root.layout.height + DockAreaHeight)));
            var newVec = new Vector2((graphPos.x - c_offsetX) / c_width, (((graphPos.y - c_offsetY) / c_height) * 2 ) -1);
            return newVec;
        }

        private Vector3 GetAbsolutePos(float x, float y)
        {   
            // Expand y-range from [0, 1] to [-1, 1]
            y = (y + 1) / 2;
            return new Vector3((x * c_width) + c_offsetX, (y * c_height) + c_offsetY, 0);
        }

        private void OnMarkerElementPositionChanged(MouseMoveEvent evt)
        {
            if (!_isDragging || GraphData == null || _activeDragElement == null)
                return;
            var graphPos = GetGraphPosition(evt.mousePosition);
            if (SnapGrid)
            {
                var inc = 1f / _gridLines;
                var snapX = MathUtils.RoundPos(graphPos.x / inc) * inc;
                var snapY = MathUtils.RoundPos(graphPos.y / inc) * inc;
                graphPos.x = snapX;
                graphPos.y = snapY;
            }

            var result = GraphData[_activeDragElement.GraphIndex].MoveTimelineNode(_activeDragElement.Index, graphPos);
            var newGraphPos = GetAbsolutePos(result.x, result.y);
            var globalPos = new Vector2(newGraphPos.x * _root.layout.width, newGraphPos.y * (_root.layout.height + DockAreaHeight));
            globalPos = this.WorldToLocal(globalPos);
            _activeDragElement.ShowData(result, true);

            _activeDragElement.style.left = globalPos.x - (_activeDragElement.layout.width / 2) - 2;
            _activeDragElement.style.bottom = globalPos.y + (_activeDragElement.layout.height / 2) + 10;
        }

        private void OnDragStart(MouseDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;
            if (evt.target is not NodeElement dragElement)
                return;
            _activeDragElement = dragElement;
            _isDragging = true;
        }

        private void OnDragEnd(MouseUpEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse || _activeDragElement == null)
                return;
            _activeDragElement.ShowData(Vector2.zero, false);
            _activeDragElement = null;
            _isDragging = false;
            OnGraphModified?.Invoke();
        }
        private void CreateNodeMarkers()
        {
            for (int i = 0; i < _positionMarkers.Length; i++)
            {
                var marker = new NodeElement();
                marker.RegisterCallback<MouseDownEvent>(OnDragStart);
                marker.RegisterCallback<MouseUpEvent>(OnDragEnd);
                Add(marker);
                _positionMarkers[i] = marker;
            }
        }

    }
}

