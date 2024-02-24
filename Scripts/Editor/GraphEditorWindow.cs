using Aikom.FunctionalAnimation.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class GraphEditorWindow : EditorWindow, IGraphController
    {
        [SerializeField] private GraphRenderElement _renderElement;
        [SerializeField] private VisualElement _selector;
        [SerializeField] private VisualElement _sideBar;

        public GraphData GraphData { get; private set; }
        public SerializedProperty GraphProperty { get; private set; }

        [MenuItem("Window/Functional Animation/Graph Editor")]
        public static void Init()
        {
            var window = GetWindow<GraphEditorWindow>();
            window.titleContent = new GUIContent("Graph Editor");
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // Side bar
            _sideBar = new VisualElement();
            _sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            _sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            _sideBar.style.flexDirection = FlexDirection.Column;
            root.Add(_sideBar);

            // Render element
            _renderElement = new GraphRenderElement(root, "Graph", new string[1] { "" });
            root.Add(_renderElement);

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

            _sideBar.Add(header);
            _sideBar.Add(subHeaderGraphSettings);
            _sideBar.Add(graphVertexCountSlider);
            _sideBar.Add(gridLineAmountSlider);

            _renderElement.RegisterCallback<MouseUpEvent>(CreateFunctionMenu);

            void CreateFunctionMenu(MouseUpEvent evt)
            {
                if (evt.button != (int)MouseButton.RightMouse)
                    return;

                var targetElement = evt.target as GraphRenderElement;
                if (targetElement == null)
                    return;

                var menu = new GenericMenu();

                var menuPosition = evt.mousePosition;
                menuPosition = root.parent.LocalToWorld(menuPosition);
                var menuRect = new Rect(menuPosition, Vector2.zero);

                //Add functions to menu
                var functionNames = (Function[])Enum.GetValues(typeof(Function));
                for (int i = 0; i < functionNames.Length; i++)
                {
                    var func = functionNames[i];
                    menu.AddItem(new GUIContent(func.ToString()), false, value => AddFunction((FunctionPosition)value), new FunctionPosition(menuPosition, func));
                }
                menu.DropDown(menuRect);
            }

            
        }

        private void AddFunction(FunctionPosition pos)
        {
            if (GraphData == null)
                return;

            var realPos = _renderElement.GetGraphPosition(pos.Position);
            
            GraphData.AddFunction(pos.Function, realPos);
            _renderElement.SetNodeMarkers();
            Refresh();
        }

        private void CreateSelector()
        {
            _selector = new VisualElement();
            var selectorheader = UIExtensions.CreateElement<Label>(_selector);
            selectorheader.text = "Change or remove functions";
            selectorheader.style.flexDirection = FlexDirection.Row;
            selectorheader.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));

            int index = 0;
            foreach (var dataPoint in GraphData.Functions)
            {
                var subSelector = new GraphFunctionController<Axis>.FunctionSelectionField(index.ToString() + ".", dataPoint);
                subSelector.Index = index;
                subSelector.Parent = this;
                subSelector.RegisterValueChangedCallback(ChangeFunction);
                subSelector.OnFunctionRemovedInUI += Refresh;
                _selector.Add(subSelector);
                index++;
            }
            _sideBar.Add(_selector);

            void ChangeFunction(ChangeEvent<Enum> evt)
            {
                var target = evt.target as GraphFunctionController<Axis>.FunctionSelectionField;
                GraphData.ChangeFunction(target.Index, (Function)evt.newValue);
            }
        }

        private struct FunctionPosition
        {
            public Vector2 Position;
            public Function Function;

            public FunctionPosition(Vector2 position, Function function)
            {
                Position = position;
                Function = function;
            }
        }

        public void SetData(GraphData data, SerializedProperty prop)
        {   
            if(data == null || data.Length == 0)
                data = new GraphData();
            GraphData = data;
            GraphProperty = prop;
            _renderElement.SetDrawTargets(0, GraphData);
            _renderElement.SetLegendHeader(prop.serializedObject.targetObject.name + " " + prop.name);
            Refresh();
        }

        public GraphData GetSource() => GraphData;
                                    

        public void Refresh()
        {   
            if(_selector != null)
                _sideBar.Remove(_selector);
            CreateSelector();
        }
    }
}

