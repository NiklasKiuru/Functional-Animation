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
        private readonly Dictionary<string, Axis> _dropDownSelectorMapping = new Dictionary<string, Axis>
        {
            { "X", Axis.X },
            { "Y", Axis.Y },
            { "Z", Axis.Z },
            { "All", Axis.W }
        };

        private GraphRenderElement _renderElement;
        private Action _unregisterCbs;
        private GraphFunctionController<Axis> _funcSelector;
        private string _currentFilePath;
        private EnumField _propertySelector;
        private static TransformAnimation _targetAnim;
        private AnimationData[] _fallbackData = new AnimationData[3] { new AnimationData(), new AnimationData(), new AnimationData() };
        private TransformProperty _selectedProperty;

        [MenuItem("Window/Functional Animation/Transform Animation Graph")]
        public static void Init()
        {
            var window = GetWindow<TransformAnimationWindow>();
            window.titleContent = new GUIContent("Transform Animation");
        }

        private void CreateGUI()
        {   
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/GraphWindowStyles.uss");
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // Side bar
            var sideBar = new VisualElement();
            sideBar.style.width = new StyleLength(new Length(20f, LengthUnit.Percent));
            sideBar.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));
            sideBar.style.flexDirection = FlexDirection.Column;
            root.Add(sideBar);

            // Render element
            _renderElement = new GraphRenderElement(root);
            _renderElement.DrawProperty = _selectedProperty;
            root.Add(_renderElement);

            if (EditorPrefs.HasKey("HeldAnimationPath"))
            {
                _currentFilePath = EditorPrefs.GetString("HeldAnimationPath");
                if (!string.IsNullOrEmpty(_currentFilePath) && _targetAnim == null)
                {
                    _targetAnim = AssetDatabase.LoadAssetAtPath<TransformAnimation>(_currentFilePath);
                    _renderElement.Animation = _targetAnim;
                }
            }

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
            var saveButton = new Button { text = "Save" };
            var saveAsButton = new Button { text = "Save As" };

            buttonContainer.Add(createNewButton);
            buttonContainer.Add(saveButton);
            buttonContainer.Add(saveAsButton);

            var objField = new ObjectField("Target Animation");
            objField.objectType = typeof(TransformAnimation);
            var maxDuration = new FloatField("Max duration");
            
            _propertySelector = new EnumField("Target Property", _selectedProperty);
            var optionsContainer = new VisualElement();
            optionsContainer.style.flexDirection = FlexDirection.Column;
            optionsContainer.style.justifyContent = Justify.SpaceAround;

            var animateToggle = new Toggle("Animate");
            var syncToggle = new Toggle("Syncronize");
            var axisSeparationToggle = new Toggle("Separate Axes");
            var axisDurationField = new FloatField("Duration");
            var timeControlField = new EnumField("Time control", TimeControl.OneShot);
            var modeControlField = new EnumField("Mode", AnimationMode.Relative);
            var animateAxisLabel = new Label("Animateable Axes");
            var animateAxisParent = new VisualElement();
            animateAxisParent.style.flexDirection = FlexDirection.Row;
            var dummyAxis = new VisualElement();
            dummyAxis.style.width = new StyleLength(new Length(153f, LengthUnit.Pixel));
            var childContainer = new VisualElement();
            childContainer.style.flexDirection = FlexDirection.Row;
            childContainer.style.justifyContent = Justify.SpaceBetween;
            childContainer.style.width = new StyleLength(new Length(50f, LengthUnit.Percent));
            var animateAxisX = new Toggle("X");
            animateAxisX.labelElement.style.minWidth = new StyleLength(new Length(10f, LengthUnit.Pixel));
            var animateAxisY = new Toggle("Y");
            animateAxisY.labelElement.style.minWidth = new StyleLength(new Length(10f, LengthUnit.Pixel));
            var animateAxisZ = new Toggle("Z");
            animateAxisZ.labelElement.style.minWidth = new StyleLength(new Length(10f, LengthUnit.Pixel));
            var offsetField = new Vector3Field("Offset");
            var startField = new Vector3Field("Start");
            var targetField = new Vector3Field("Target");

            ReadTargetValues();

            childContainer.Add(animateAxisX);
            childContainer.Add(animateAxisY);
            childContainer.Add(animateAxisZ);

            animateAxisParent.Add(dummyAxis);
            animateAxisParent.Add(childContainer);
            
            optionsContainer.Add(animateToggle);
            optionsContainer.Add(syncToggle);
            optionsContainer.Add(axisSeparationToggle);
            optionsContainer.Add(axisDurationField);
            optionsContainer.Add(timeControlField);
            optionsContainer.Add(modeControlField);
            optionsContainer.Add(animateAxisLabel);
            optionsContainer.Add(animateAxisParent);
            optionsContainer.Add(offsetField);
            optionsContainer.Add(startField);
            optionsContainer.Add(targetField);

            // Debug window
            if (_targetAnim != null)
                _funcSelector = new GraphFunctionController<Axis>(_targetAnim[0], Axis.X, _dropDownSelectorMapping);
            else
                _funcSelector = new GraphFunctionController<Axis>(_fallbackData[0], Axis.X, _dropDownSelectorMapping) ;

            // Tree construction
            sideBar.Add(header);
            sideBar.Add(subHeaderGraphSettings);
            sideBar.Add(graphVertexCountSlider);
            sideBar.Add(gridLineAmountSlider);
            sideBar.Add(animatorHeader);
            sideBar.Add(buttonContainer);
            sideBar.Add(objField);
            sideBar.Add(maxDuration);
            sideBar.Add(_propertySelector);
            sideBar.Add(optionsContainer);
            sideBar.Add(_funcSelector);
            

            // Register callbacks and set a fallback method for unbinds
            RegisterCallbacks();
            ReadTargetValues();
            AssignTargetProperty((TransformProperty)_propertySelector.value);
            ChangeControlAxis((AnimationMode)modeControlField.value);
            _unregisterCbs = UnRegisterCallbacks;

            void ReadTargetValues()
            {
                if (_targetAnim == null)
                    return;
                var targetProp = _targetAnim[_selectedProperty];
                _renderElement.Animation = _targetAnim;
                objField.value = _targetAnim;
                maxDuration.value = _targetAnim.Duration;
                animateToggle.value = targetProp.Animate;
                syncToggle.value = targetProp.Sync;
                axisSeparationToggle.value = targetProp.SeparateAxis;
                axisDurationField.value = targetProp.Duration;
                timeControlField.value = targetProp.TimeControl;
                modeControlField.value = targetProp.Mode;
                animateAxisX.value = targetProp.AnimateableAxis.x;
                animateAxisY.value = targetProp.AnimateableAxis.y;
                animateAxisZ.value = targetProp.AnimateableAxis.z;
                startField.value = targetProp.Start;
                targetField.value = targetProp.Target;
                offsetField.value = targetProp.Offset;
            }

            // Graph settings callbacks
            void ChangeVertexCount(ChangeEvent<int> e) => _renderElement.SampleAmount = e.newValue;
            void ChangeGridLineCount(ChangeEvent<int> e) => _renderElement.GridLines = e.newValue;

            // Animation callbacks
            void ChangeControlMode(ChangeEvent<Enum> e)
            {
                if(_targetAnim == null)
                    return;
                var val = (AnimationMode)e.newValue;
                ChangeControlAxis(val);

                _targetAnim[_selectedProperty].Mode = val;
            }

            void ChangeControlAxis(AnimationMode mode)
            {
                if (mode == AnimationMode.Absolute)
                {
                    targetField.SetEnabled(true);
                    startField.SetEnabled(true);
                    offsetField.SetEnabled(false);
                }
                else
                {
                    targetField.SetEnabled(false);
                    startField.SetEnabled(false);
                    offsetField.SetEnabled(true);
                }
            }

            void ChangeStart(ChangeEvent<Vector3> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].Start = e.newValue;
            }

            void ChangeTarget(ChangeEvent<Vector3> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].Target = e.newValue;
            }

            void ChangeMaxDuration(ChangeEvent<float> e)
            {   
                if (_targetAnim == null)
                    return;
                _targetAnim.Duration = e.newValue;
            }  
            
            void ChangeAnimate(ChangeEvent<bool> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].Animate = e.newValue;
            }

            void ChangeSync(ChangeEvent<bool> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].Sync = e.newValue;
                if(e.newValue)
                    _renderElement.XAxisMultiplier = _targetAnim.Duration;
                else
                    _renderElement.XAxisMultiplier = 1f;
            }

            void ChangeSeparateAxis(ChangeEvent<bool> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].SeparateAxis = e.newValue;
                if (!e.newValue)
                    _funcSelector.LockSelector(Axis.W);
                else
                    _funcSelector.UnlockSelector(Axis.W);
            }

            void ChangeDuration(ChangeEvent<float> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].Duration = e.newValue;
            }

            void ChangeTimeControl(ChangeEvent<Enum> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].TimeControl = (TimeControl)e.newValue;
            }

            void ChangeAnimateableAxis(ChangeEvent<bool> e)
            {
                if (_targetAnim == null)
                    return;
                var prop = _targetAnim[_selectedProperty];
                var axis = prop.AnimateableAxis;
                var index = (e.target as Toggle).label switch
                {
                    "X" => 0,
                    "Y" => 1,
                    "Z" => 2,
                    _ => throw new System.IndexOutOfRangeException(),
                };
                axis[index] = e.newValue;
                prop.AnimateableAxis = axis;
            }

            void SetOffset(ChangeEvent<Vector3> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].Offset = e.newValue;
            }

            void ChangeTargetAnimation(ChangeEvent<UnityEngine.Object> e)
            {
                _targetAnim = e.newValue as TransformAnimation;
                _renderElement.Animation = _targetAnim;
                _selectedProperty = TransformProperty.Position;
                _renderElement.DrawProperty = _selectedProperty;
                var axis = _funcSelector.CurrentSelection;
                _renderElement.SetNodeMarkers(axis);
                if(_targetAnim != null)
                    _funcSelector.OverrideTargetContainer(_targetAnim[_selectedProperty]);
                else
                    _funcSelector.OverrideTargetContainer(_fallbackData[(int)_selectedProperty]);
                ReadTargetValues();
            }
            void CreateNewAnimation()
            {
                var path = EditorUtility.SaveFilePanelInProject("Save animation", "New Animation", "asset", "Save animation");
                
                if (string.IsNullOrEmpty(path))
                    return;
                _targetAnim = TransformAnimation.SaveNew(path);
                objField.value = _targetAnim;
                _renderElement.Animation = _targetAnim;
                var axis = _funcSelector.CurrentSelection;
                _renderElement.SetNodeMarkers(axis);
                ReadTargetValues();
            }

            void SetTargetProperty(ChangeEvent<Enum> e) 
            {   
                AssignTargetProperty((TransformProperty)e.newValue);
                ReadTargetValues();
            }

            void RegisterCallbacks()
            {
                graphVertexCountSlider.RegisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.RegisterValueChangedCallback(ChangeGridLineCount);
                objField.RegisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked += CreateNewAnimation;
                _renderElement.RegisterCallback<MouseUpEvent>(CreateFunctionMenu);
                _propertySelector.RegisterValueChangedCallback(SetTargetProperty);
                _funcSelector.CurrentSelectionChanged += _renderElement.SetNodeMarkers;
                GraphFunctionController<Axis>.FunctionSelectionField.OnFunctionRemovedInUI += _renderElement.SetNodeMarkers;
                maxDuration.RegisterValueChangedCallback(ChangeMaxDuration);
                animateToggle.RegisterValueChangedCallback(ChangeAnimate);
                syncToggle.RegisterValueChangedCallback(ChangeSync);
                axisSeparationToggle.RegisterValueChangedCallback(ChangeSeparateAxis);
                axisDurationField.RegisterValueChangedCallback(ChangeDuration);
                timeControlField.RegisterValueChangedCallback(ChangeTimeControl);
                animateAxisX.RegisterValueChangedCallback(ChangeAnimateableAxis);
                animateAxisY.RegisterValueChangedCallback(ChangeAnimateableAxis);
                animateAxisZ.RegisterValueChangedCallback(ChangeAnimateableAxis);
                offsetField.RegisterValueChangedCallback(SetOffset);
                modeControlField.RegisterValueChangedCallback(ChangeControlMode);
                startField.RegisterValueChangedCallback(ChangeStart);
                targetField.RegisterValueChangedCallback(ChangeTarget);
            }

            void UnRegisterCallbacks()
            {
                graphVertexCountSlider.UnregisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.UnregisterValueChangedCallback(ChangeGridLineCount);
                objField.UnregisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked -= CreateNewAnimation;
                _renderElement.UnregisterCallback<MouseUpEvent>(CreateFunctionMenu);
                _propertySelector.UnregisterValueChangedCallback(SetTargetProperty);
                _funcSelector.CurrentSelectionChanged -= _renderElement.SetNodeMarkers;
                GraphFunctionController<Axis>.FunctionSelectionField.OnFunctionRemovedInUI -= _renderElement.SetNodeMarkers;
                maxDuration.UnregisterValueChangedCallback(ChangeMaxDuration);
                animateToggle.UnregisterValueChangedCallback(ChangeAnimate);
                syncToggle.UnregisterValueChangedCallback(ChangeSync);
                axisSeparationToggle.UnregisterValueChangedCallback(ChangeSeparateAxis);
                axisDurationField.UnregisterValueChangedCallback(ChangeDuration);
                timeControlField.UnregisterValueChangedCallback(ChangeTimeControl);
                animateAxisX.UnregisterValueChangedCallback(ChangeAnimateableAxis);
                animateAxisY.UnregisterValueChangedCallback(ChangeAnimateableAxis);
                animateAxisZ.UnregisterValueChangedCallback(ChangeAnimateableAxis);
                offsetField.UnregisterValueChangedCallback(SetOffset);
                modeControlField.UnregisterValueChangedCallback(ChangeControlMode);
                startField.UnregisterValueChangedCallback(ChangeStart);
                targetField.UnregisterValueChangedCallback(ChangeTarget);
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

        private void AssignTargetProperty(TransformProperty prop)
        {   
            _selectedProperty = prop;

            if(_targetAnim == null)
                return;
            _renderElement.DrawProperty = prop;
            _renderElement.XAxisMultiplier = _targetAnim[prop].Sync ? _targetAnim.Duration : 1f;
            _funcSelector.OverrideTargetContainer(_targetAnim[prop]);
            if (_targetAnim[_selectedProperty].SeparateAxis)
                _funcSelector.UnlockSelector(_funcSelector.CurrentSelection);
            else
                _funcSelector.LockSelector(Axis.W);

            _renderElement.SetNodeMarkers(_funcSelector.CurrentSelection);
        }
        

        private void AddFunction(FunctionPosition pos)
        {
            if (_targetAnim == null)
                return;

            var realPos = _renderElement.GetGraphPosition(pos.Position);
            var container = _targetAnim[_selectedProperty];
            container.AddFunction(pos.Function, _funcSelector.CurrentSelection, realPos);
            _funcSelector.Refresh();
            _renderElement.SetNodeMarkers(_funcSelector.CurrentSelection);

            Debug.Log("Selected: " + pos.Function.ToString());
            Debug.Log("Calculated Position: " + realPos.ToString());
            Debug.Log("Event position: " + pos.Position.ToString());
        }

        void OnLostFocus()
        {   
            _currentFilePath = _targetAnim != null ? AssetDatabase.GetAssetPath(_targetAnim) : null;
            EditorPrefs.SetString("HeldAnimationPath", _currentFilePath);
        }

        void OnDestroy()
        {
            _currentFilePath = _targetAnim != null ? AssetDatabase.GetAssetPath(_targetAnim) : null;
            EditorPrefs.SetString("HeldAnimationPath", _currentFilePath);
        }

        private void OnDisable()
        {
            _currentFilePath = _targetAnim != null ? AssetDatabase.GetAssetPath(_targetAnim) : null;
            EditorPrefs.SetString("HeldAnimationPath", _currentFilePath);
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

