using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using Aikom.FunctionalAnimation.UI;
using System.Runtime.InteropServices.WindowsRuntime;

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
        private GraphFunctionController _funcSelector;
        private string _currentFilePath;

        // **TEST**
        private static TransformAnimation _targetAnim;
        private AnimationData[] _fallbackData = new AnimationData[3] { new AnimationData(), new AnimationData(), new AnimationData() };
        private TransformProperty _selectedProperty;

        [MenuItem("Window/Functional Animation")]
        public static void Init()
        {
            var window = GetWindow<GraphWindow>();
            window.titleContent = new GUIContent("Graph Window");
        }

        private void CreateGUI()
        {   
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/GraphWindowStyles.uss");
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;

            // Render element
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/UI/GraphMaterial.mat");
            _renderElement = new GraphRenderElement(material, root);
            _renderElement.style.width = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.height = new StyleLength(new Length(80f, LengthUnit.Percent));
            _renderElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            _renderElement.style.borderLeftWidth = new StyleFloat(2);
            _renderElement.style.borderBottomWidth = new StyleFloat(2);
            _renderElement.style.borderLeftColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            _renderElement.style.borderBottomColor = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            _renderElement.DrawProperty = _selectedProperty;

            if (EditorPrefs.HasKey("HeldAnimationPath"))
            {
                _currentFilePath = EditorPrefs.GetString("HeldAnimationPath");
                if (!string.IsNullOrEmpty(_currentFilePath) && _targetAnim == null)
                {
                    _targetAnim = AssetDatabase.LoadAssetAtPath<TransformAnimation>(_currentFilePath);
                    _renderElement.Animation = _targetAnim;
                }
            }

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
            
            var propSelector = new EnumField("Target Property", _selectedProperty);
            var optionsContainer = new VisualElement();
            optionsContainer.style.flexDirection = FlexDirection.Column;
            optionsContainer.style.justifyContent = Justify.SpaceAround;

            var animateToggle = new Toggle("Animate");
            var syncToggle = new Toggle("Syncronize");
            var axisSeparationToggle = new Toggle("Separate Axes");
            var axisDurationField = new FloatField("Duration");
            var timeControlField = new EnumField("Time control", TimeControl.OneShot);
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
            optionsContainer.Add(animateAxisLabel);
            optionsContainer.Add(animateAxisParent);
            optionsContainer.Add(offsetField);

            // Debug window
            if(_targetAnim != null)
                _funcSelector = new GraphFunctionController(_targetAnim.AnimationData[0], Function.Linear, Axis.X);
            else
                _funcSelector = new GraphFunctionController(_fallbackData[0], Function.Linear, Axis.X);

            // Tree construction
            sideBar.Add(header);
            sideBar.Add(subHeaderGraphSettings);
            sideBar.Add(graphVertexCountSlider);
            sideBar.Add(gridLineAmountSlider);
            sideBar.Add(animatorHeader);

            // **OLD**
            //sideBar.Add(headerContainer);
            //sideBar.Add(positionInfoContainer);
            //sideBar.Add(rotationInfoContainer);
            //sideBar.Add(scaleInfoContainer);
            //sideBar.Add(_scriptableElement);


            sideBar.Add(buttonContainer);
            sideBar.Add(objField);
            sideBar.Add(maxDuration);
            sideBar.Add(propSelector);
            sideBar.Add(optionsContainer);
            sideBar.Add(_funcSelector);
            root.Add(sideBar);
            root.Add(_renderElement);

            // Register callbacks and set a fallback method for unbinds
            //Selection.selectionChanged += SetAnimatior;
            _renderElement.CreateNodeMarkers();
            RegisterCallbacks();
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
                animateAxisX.value = targetProp.AnimateableAxis.x;
                animateAxisY.value = targetProp.AnimateableAxis.y;
                animateAxisZ.value = targetProp.AnimateableAxis.z;
                offsetField.value = targetProp.Offset;
            }

            // Graph settings callbacks
            void ChangeVertexCount(ChangeEvent<int> e) => _renderElement.SampleAmount = e.newValue;
            void ChangeGridLineCount(ChangeEvent<int> e) => _renderElement.GridLines = e.newValue;

            // Animation callbacks
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
            }

            void ChangeSeparateAxis(ChangeEvent<bool> e)
            {
                if (_targetAnim == null)
                    return;
                _targetAnim[_selectedProperty].SeparateAxis = e.newValue;
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
                var axis = _funcSelector.CurrentAxis;
                _renderElement.SetNodeMarkers(axis);
                if(_targetAnim != null)
                    _funcSelector.OverrideTargetContainer(_targetAnim.AnimationData[(int)_selectedProperty]);
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
                var axis = _funcSelector.CurrentAxis;
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
                propSelector.RegisterValueChangedCallback(SetTargetProperty);
                _funcSelector.CurrentAxisChanged += _renderElement.SetNodeMarkers;
                GraphFunctionController.FunctionSelectionField.OnFunctionRemovedInUI += _renderElement.SetNodeMarkers;
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
            }

            void UnRegisterCallbacks()
            {
                graphVertexCountSlider.UnregisterValueChangedCallback(ChangeVertexCount);
                gridLineAmountSlider.UnregisterValueChangedCallback(ChangeGridLineCount);
                objField.UnregisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked -= CreateNewAnimation;
                _renderElement.UnregisterCallback<MouseUpEvent>(CreateFunctionMenu);
                propSelector.UnregisterValueChangedCallback(SetTargetProperty);
                _funcSelector.CurrentAxisChanged -= _renderElement.SetNodeMarkers;
                GraphFunctionController.FunctionSelectionField.OnFunctionRemovedInUI -= _renderElement.SetNodeMarkers;
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

        
        private void AssignTargetProperty(TransformProperty prop)
        {   
            if(_selectedProperty == prop)
                return;

            _renderElement.DrawProperty = prop;
            _selectedProperty = prop;
            _funcSelector.OverrideTargetContainer(_targetAnim.AnimationData[(int)prop]);
            _renderElement.SetNodeMarkers(_funcSelector.CurrentAxis);
        }
        

        private void AddFunction(FunctionPosition pos)
        {
            if (_targetAnim == null)
                return;

            var realPos = _renderElement.GetGraphPosition(pos.Position);
            var container = _targetAnim.AnimationData[(int)_selectedProperty];
            container.AddFunction(pos.Function, _funcSelector.CurrentAxis, realPos);
            _funcSelector.Refresh();
            _renderElement.SetNodeMarkers(_funcSelector.CurrentAxis);

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
            //Selection.selectionChanged -= SetAnimatior;
            _unregisterCbs?.Invoke();
        }

        //private void SetAnimatior()
        //{
        //    var obj = Selection.activeGameObject;
        //    if (obj != null && obj.TryGetComponent<TransformAnimator>(out var anim))
        //    {
        //        _renderElement.Animation = anim;
        //        if(anim is ScriptableTransformAnimator)
        //        {
        //            _scriptableElement.style.visibility = Visibility.Visible;
        //            _scriptableElement.Q<Button>().clicked += LoadAnimation;
        //        }
        //    }
        //    else
        //    {
        //        _renderElement.Animation = null;
        //        UnBindLoadButton();
        //    }
        //}

        //private void UnBindLoadButton()
        //{
        //    _scriptableElement.style.visibility = Visibility.Hidden;
        //    _scriptableElement.Q<Button>().clicked -= LoadAnimation;
        //}

        //private void LoadAnimation()
        //{
        //    var name = _scriptableElement.Q<TextField>().value;
        //    var scriptableAnimator = _renderElement.Animation as ScriptableTransformAnimator;
        //    if (scriptableAnimator != null || !string.IsNullOrEmpty(name))
        //        scriptableAnimator.Play(name);
        //}

        //private void OnInspectorUpdate()
        //{
        //    if(_renderElement.Animation != null && !Application.isPlaying)
        //        Repaint();
        //    else if(_renderElement.Animation != null && Application.isPlaying)
        //    {
        //        for (int i = 0; i < 3; i++)
        //        {
        //            var container = _renderElement.Animation.Container[i];
        //            if (container != null)
        //            {   
        //                var time = container.Time;
        //                _normalTimeValues[i].value = time.ToString("F2");
        //                _accelerationValues[i].value = (MathUtils.Derivate(container.EasingFunc, 
        //                    time, _renderElement.MeasurementInterval) * container.Direction).ToString("F2");
        //            }

        //        }
        //    }
                
        //}

        private void Update()
        {
            if (Application.isPlaying && _renderElement.Animation != null)
            {   
                // This is for playhead update mainly
                _renderElement.DrawGraph();
                Repaint();
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

        /// <summary>
        /// Visual element that draws the animation graph
        /// Drawing is currently done by using GL calls and I might update this in the future to use the UIElements Mesh API instead for more flexibility
        /// </summary>
        private class GraphRenderElement : ImmediateModeElement
        {
            private const int c_maxGridLines = 20;

            private Material _graphMaterial;
            private int _sampleAmount = 1000;
            private int _gridLines = 10;
            private Label[] _gridMarkers = new Label[c_maxGridLines];
            private Label[] _gridTimeMarkers = new Label[c_maxGridLines];
            private int _currentMarkers = 0;
            private float _measurementInterval;
            private Label _propertyName;
            private NodeElement[] _positionMarkers = new NodeElement[30];
            private NodeElement _activeDragElement;
            private VisualElement _root;
            private VisualElement _dockWindow;
            private bool _isDragging = false;

            /// <summary>
            /// Maximum allowed grid lines
            /// </summary>
            public static int MaxGridLines { get => c_maxGridLines; }

            /// <summary>
            /// Target animation this element draws the graph from
            /// </summary>
            public TransformAnimation Animation { get; set; }

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
            /// Currently drawn property
            /// </summary>
            public TransformProperty DrawProperty { get; set; }

            /// <summary>
            /// Base constructor
            /// </summary>
            /// <param name="lineMaterial"></param>
            /// <param name="root"></param>
            public GraphRenderElement(Material lineMaterial, VisualElement root)
            {
                _graphMaterial = lineMaterial;
                _root = root;
                _dockWindow = root.parent;
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
                mainContainer.style.maxWidth = new StyleLength(new Length(40f, LengthUnit.Pixel));
                
                _propertyName = new Label(DrawProperty.ToString());
                mainContainer.Add(_propertyName);
                Add(mainContainer);

                CreateLegend("X", Color.red);
                CreateLegend("Y", Color.green);
                CreateLegend("Z", Color.blue);
                CreateLegend("All", Color.white);

                RegisterCallback<MouseMoveEvent>(OnMarkerElementPositionChanged);
                RegisterCallback<MouseUpEvent>(OnDragEnd);

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
            /// <summary>
            /// Creates initial node markers
            /// </summary>
            public void CreateNodeMarkers()
            {
                for (int i = 0; i < _positionMarkers.Length; i++)
                {
                    var marker = new NodeElement();
                    marker.RegisterCallback<MouseDownEvent>(OnDragStart);
                    marker.RegisterCallback<MouseUpEvent>(OnDragEnd);
                    _root.Add(marker);
                    _positionMarkers[i] = marker;
                }
            }

            /// <summary>
            /// Sets and activates the node markers for the given axis
            /// </summary>
            /// <param name="axis"></param>
            public void SetNodeMarkers(Axis axis)
            {
                if (Animation == null)
                    return;
                for(int i = 0; i < _positionMarkers.Length; i++)
                    _positionMarkers[i].style.visibility = Visibility.Hidden;

                var axisColor = GetAxisColor(axis);
                var activeMarkers = 0;
                var container = Animation.AnimationData[(int)DrawProperty];
                var nodes = container[axis].Nodes;
                foreach(var node in nodes)
                {
                    var pos = GetAbsolutePos(node.x, node.y);
                    var marker = _positionMarkers[activeMarkers];
                    marker.style.visibility = Visibility.Visible;
                    marker.style.left = pos.x * _dockWindow.layout.width - (marker.layout.width / 2);
                    marker.style.bottom = pos.y * _dockWindow.layout.height - (marker.layout.height / 2);
                    marker.Activate(activeMarkers, axisColor, axis);
                    activeMarkers++;
                }
            }

            
            /// <summary>
            /// Gets the position on the graph from the given panel position
            /// </summary>
            /// <param name="panelPositon"></param>
            /// <returns></returns>
            public Vector2 GetGraphPosition(Vector2 panelPositon)
            {
                var graphPos = new Vector2(panelPositon.x / _dockWindow.layout.width, 1 - (panelPositon.y / _dockWindow.layout.height));
                var newVec = new Vector2((graphPos.x - 0.2f) / 0.75f, (graphPos.y - 0.2f) / 0.7f);
                return newVec;
            }

            /// <summary>
            /// Main method for drawing the graph
            /// </summary>
            public void DrawGraph()
            {
                if (Animation == null)
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
                    var interval = 1f / _gridLines;
                    for (int j = 1; j <= _gridLines; j++)
                    {
                        float x = j * interval;

                        // X-axis
                        DrawVertex(x, 0);
                        DrawVertex(x, 1);
                        var timeMarker = _gridTimeMarkers[j - 1];
                        timeMarker.style.visibility = Visibility.Visible;
                        var duration = Animation[DrawProperty].Sync ? Animation.Duration : Animation[DrawProperty].Duration;
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

                    if (_gridLines > _currentMarkers)
                        _currentMarkers = _gridLines;
                    else
                        DisableInactiveMarkers();

                    var container = Animation.AnimationData[(int)DrawProperty];
                    var axisCount = container.Length - 1;
                    int startingPoint = 0;
                    if (!container.SeparateAxis)
                    {
                        startingPoint = 3;
                        axisCount++;
                    }

                    var currentPositions = new Vector3[axisCount - startingPoint];
                    _propertyName.text = DrawProperty.ToString();
                    
                    while (startingPoint < axisCount)
                    {
                        SetColor(startingPoint);
                        var func = container.GenerateFunction((Axis)startingPoint);
                        float sampleInc = 1f / _sampleAmount;
                        _measurementInterval = sampleInc;

                        //    //currentPositions[i] = GetAbsolutePos(container.Time, func(container.Time));

                        for (int sample = 0; sample < _sampleAmount; sample++)
                        {
                            DrawVertexSingle(sample);
                            DrawVertexSingle((sample + 1));
                        }
                        startingPoint++;

                        Vector3 DrawVertexSingle(int index)
                        {
                            float x = index * sampleInc;
                            return DrawVertex(x, func(x));
                        }
                    }



                    //if(Application.isPlaying)
                    //{
                    //    // Draws the playhead
                    //    GL.Color(Color.white);
                    //    DrawVertex(Animation.Timer.Time, 0);
                    //    DrawVertex(Animation.Timer.Time, 1);
                    //    GL.End();

                    //    // Draws the current time markers
                    //    GL.Begin(GL.QUADS);
                    //    var sideLen = 0.01f;
                    //    for (int i = 0; i < 3; i++)
                    //    {
                    //        if (Animation.Container[i] == null || !Animation.Container[i].Animate)
                    //            continue;
                    //        SetColor(i);
                    //        GL.Vertex3(currentPositions[i].x - sideLen, currentPositions[i].y - sideLen, 0);
                    //        GL.Vertex3(currentPositions[i].x - sideLen, currentPositions[i].y + sideLen, 0);
                    //        GL.Vertex3(currentPositions[i].x + sideLen, currentPositions[i].y + sideLen, 0);
                    //        GL.Vertex3(currentPositions[i].x + sideLen, currentPositions[i].y - sideLen, 0);
                    //    }
                    //}

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
                GL.Color(GetAxisColor((Axis)index));
            }

            private Color GetAxisColor(Axis axis)
            {
                switch (axis)
                {
                    case Axis.X:
                        return Color.red;
                    case Axis.Y:
                        return Color.green;
                    case Axis.Z:
                        return Color.blue;
                    case Axis.W:
                        return Color.white;
                }
                return Color.white;
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

            private Vector3 DrawVertex(float x, float y)
            {   
                var pos = GetAbsolutePos(x, y);
                GL.Vertex3(pos.x, pos.y, pos.z);
                return pos;
            }

            private Vector3 GetAbsolutePos(float x, float y) 
            {   
                return new Vector3((x * 0.75f) + 0.2f, (y * 0.7f) + 0.2f, 0); 
            }

            private void OnMarkerElementPositionChanged(MouseMoveEvent evt)
            {
                if (!_isDragging || Animation == null || _activeDragElement == null)
                    return;
                var pos = _dockWindow.LocalToWorld(evt.mousePosition);
                var graphPos = GetGraphPosition(pos);
                var result = Animation.AnimationData[(int)DrawProperty][_activeDragElement.Axis].MoveTimelineNode(_activeDragElement.Index, graphPos);
                var globalPos = GetAbsolutePos(result.x, result.y);
                _activeDragElement.style.left = globalPos.x * _dockWindow.layout.width - (_activeDragElement.layout.width / 2);
                _activeDragElement.style.bottom = globalPos.y * _dockWindow.layout.height - (_activeDragElement.layout.height / 2);
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
                if (evt.button != (int)MouseButton.LeftMouse)
                    return;
                _activeDragElement = null;
                _isDragging = false;
            }

        }
    }
}

