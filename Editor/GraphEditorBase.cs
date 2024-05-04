using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public abstract class GraphEditorBase : EditorWindow
    {
        [SerializeField] protected GraphRenderElement _renderElement;
        [SerializeField] protected HorizontalGroupController _sideBar;
        [SerializeField] private VisualElement _graphProperties;

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            var settings = GraphSettings.instance.GetOrLoadDefaults();

            // Side bar
            _sideBar = new HorizontalGroupController();
            _sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            _sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            _sideBar.style.flexDirection = FlexDirection.Column;
            root.Add(_sideBar);

            // Render element
            _renderElement = new GraphRenderElement(root, "Graph", new string[1] { "" }, settings);
            root.Add(_renderElement);

            _graphProperties = new VisualElement();
            

            var graphVertexCountSlider = new SliderInt("Vertex count")
            {
                highValue = 1000,
                lowValue = 100,
                value = settings.VertexCount,
                showInputField = true
            };
            graphVertexCountSlider.style.borderTopWidth = 5;

            var gridLineAmountSlider = new SliderInt("Grid line count")
            {
                highValue = GraphRenderElement.MaxGridLines,
                lowValue = 5,
                value = settings.GridLineCount,
                showInputField = true
            };

            var gridSnapElement = new Toggle("Use grid snapping");
            gridSnapElement.value = settings.SnapToGrid;

            Func<VisualElement> makeItem = () => new ColorField();
            Action<VisualElement, int> bindItem = (e, i) => (e as ColorField).value = (Color)_renderElement.LegendColors[i];

            var colorArray = new ListView(_renderElement.LegendColors, 16, makeItem, bindItem);
            colorArray.headerTitle = "Legend Colors";
            colorArray.showBorder = true;
            colorArray.showFoldoutHeader = true;
            colorArray.showAddRemoveFooter = true;

            _graphProperties.Add(graphVertexCountSlider);
            _graphProperties.Add(gridLineAmountSlider);
            _graphProperties.Add(gridSnapElement);
            _graphProperties.Add(colorArray);

            _sideBar.AddElement("Graph Settings", _graphProperties);
            _renderElement.RegisterCallback<MouseUpEvent>(CreateFunctionMenu);
            graphVertexCountSlider.RegisterValueChangedCallback(ChangeVertexCount);
            gridLineAmountSlider.RegisterValueChangedCallback(ChangeGridLineCount);
            gridSnapElement.RegisterValueChangedCallback(SetGridSnapping);

            OnAfterCreateGUI();

            // Graph settings callbacks
            void ChangeVertexCount(ChangeEvent<int> e) 
            {
                _renderElement.SampleAmount = e.newValue;
                settings.VertexCount = e.newValue;
            } 
            void ChangeGridLineCount(ChangeEvent<int> e) 
            {
                _renderElement.GridLines = e.newValue;
                settings.GridLineCount = e.newValue;
            }
            void SetGridSnapping(ChangeEvent<bool> e)
            {
                _renderElement.SnapGrid = e.newValue;
                settings.SnapToGrid = e.newValue;
            }

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
                var definitions = BurstFunctionCache.GetDefinitions().ToArray();
                for (int i = 0; i < definitions.Length; i++)
                {
                    var func = definitions[i];
                    menu.AddItem(new GUIContent(func.Value), false, value => AddFunction((FunctionPosition)value), new FunctionPosition(menuPosition, func));
                }
                menu.DropDown(menuRect);
            }
        }

        protected virtual void OnAfterCreateGUI() { }
        protected abstract void AddFunction(FunctionPosition pos);
        protected struct FunctionPosition
        {
            public Vector2 Position;
            public FunctionAlias Function;

            public FunctionPosition(Vector2 position, FunctionAlias function)
            {
                Position = position;
                Function = function;
            }
        }

        protected virtual void OnDisable() 
        {   
            GraphSettings.instance.SaveValues();
        }
    }
}
