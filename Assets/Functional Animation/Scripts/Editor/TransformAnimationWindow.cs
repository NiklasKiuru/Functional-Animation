using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Aikom.FunctionalAnimation.Editor
{
    /// <summary>
    /// This script shows the animator graph in the editor
    /// The UI building here is messy at best since I did not want to spend too much time on it
    /// </summary>
    internal class TransformAnimationWindow : EditorWindow
    {
        private readonly string[] _legendMapping = new string[4]
        {
            "X",
            "Y",
            "Z",
            "All"
        };

        private readonly Dictionary<string, TransformProperty> _propertyMapping = new Dictionary<string, TransformProperty>
        {
            { "Position", TransformProperty.Position },
            { "Rotation", TransformProperty.Rotation },
            { "Scale", TransformProperty.Scale }
        };

        [SerializeField] private GraphRenderElement _renderElement;
        [SerializeField] private Action _unregisterCbs;
        [SerializeField] private TransformAnimation _targetAnim;
        [SerializeField] private PropertySelectorElement _selector;

        [MenuItem("Window/Functional Animation/Transform Animation Graph")]
        public static void Init()
        {
            var window = GetWindow<TransformAnimationWindow>();
            window.titleContent = new GUIContent("Transform Animation");
        }

        private void CreateGUI()
        {   
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // Side bar
            var sideBar = new VisualElement();
            sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            sideBar.style.flexDirection = FlexDirection.Column;
            root.Add(sideBar);

            // Render element
            _renderElement = new GraphRenderElement(root, "Property", _legendMapping);
            //_renderElement.Animation = _targetAnim;
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

            // Animator properties
            var animatorHeader = new Label("Animator");
            animatorHeader.style.fontSize = 12;
            animatorHeader.style.marginBottom = new StyleLength(new Length(5f, LengthUnit.Pixel));
            animatorHeader.style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));

            // New Menu
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.justifyContent = Justify.Center;
            var createNewButton = new Button { text = "Create New" };
            var saveAsButton = new Button { text = "Save As" };

            buttonContainer.Add(createNewButton);
            buttonContainer.Add(saveAsButton);

            var objField = new ObjectField("Target Animation");
            objField.objectType = typeof(TransformAnimation);
            objField.value = _targetAnim;
            
            _selector = new PropertySelectorElement(_targetAnim, _propertyMapping);
            _selector.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
            
            // Tree construction
            sideBar.Add(header);
            sideBar.Add(subHeaderGraphSettings);
            sideBar.Add(graphVertexCountSlider);
            sideBar.Add(gridLineAmountSlider);
            sideBar.Add(animatorHeader);
            sideBar.Add(buttonContainer);
            sideBar.Add(objField);
            sideBar.Add(_selector);
            

            // Register callbacks and set a fallback method for unbinds
            RegisterCallbacks();
            _unregisterCbs = UnRegisterCallbacks;

            // Graph settings callbacks
            void ChangeVertexCount(ChangeEvent<int> e) => _renderElement.SampleAmount = e.newValue;
            void ChangeGridLineCount(ChangeEvent<int> e) => _renderElement.GridLines = e.newValue;

            void ChangeTargetAnimation(ChangeEvent<UnityEngine.Object> e)
            {
                _targetAnim = e.newValue as TransformAnimation;
                _selector.OverrideTargetContainer(_targetAnim);
                SetDrawProperty(_selector.CurrentSelection);
            }
            void CreateNewAnimation()
            {
                var path = EditorUtility.SaveFilePanelInProject("Save animation", "New Animation", "asset", "Save animation");
                
                if (string.IsNullOrEmpty(path))
                    return;
                _targetAnim = TransformAnimation.SaveNew(path);
                objField.value = _targetAnim;
                _selector.OverrideTargetContainer(_targetAnim);
                SetDrawProperty(_selector.CurrentSelection);
            }

            void RegisterCallbacks()
            {
                graphVertexCountSlider.RegisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.RegisterValueChangedCallback(ChangeGridLineCount);
                objField.RegisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked += CreateNewAnimation;
                _renderElement.RegisterCallback<MouseUpEvent>(CreateFunctionMenu);
                _selector.OnSelectionChanged += SetDrawProperty;
            }

            void UnRegisterCallbacks()
            {
                graphVertexCountSlider.UnregisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.UnregisterValueChangedCallback(ChangeGridLineCount);
                objField.UnregisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked -= CreateNewAnimation;
                _renderElement.UnregisterCallback<MouseUpEvent>(CreateFunctionMenu);
                _selector.OnSelectionChanged -= SetDrawProperty;
            }

            void SetDrawProperty(TransformProperty prop)
            {
                if (_targetAnim == null)
                {
                    _renderElement.SetDrawTargets(0, null);
                    return;
                }
                var container = _targetAnim[prop];
                
                _renderElement.SetDrawTargets((int)_selector.CurrentAxis, container[Axis.X], container[Axis.Y], container[Axis.Z], container[Axis.W]);
                _renderElement.SetLegendHeader(prop.ToString());
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
                var functionNames = (Function[])Enum.GetValues(typeof(Function));
                for(int i = 0; i < functionNames.Length; i++)
                {   
                    var func = functionNames[i];
                    menu.AddItem(new GUIContent(func.ToString()), false, value => AddFunction((FunctionPosition)value), new FunctionPosition(menuPosition, func));
                }
                menu.DropDown(menuRect);
            }
        }

        private void AddFunction(FunctionPosition pos)
        {
            if (_targetAnim == null)
                return;

            var realPos = _renderElement.GetGraphPosition(pos.Position);
            var container = _targetAnim[_selector.CurrentSelection];
            container.AddFunction(pos.Function, _selector.CurrentAxis, realPos);
            _renderElement.SetNodeMarkers();
            _selector.AxisController.Refresh();
        }

        private void OnDisable()
        {
            _unregisterCbs?.Invoke();
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

        
    }
}

