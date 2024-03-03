using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation.Editor
{
    /// <summary>
    /// This script shows the animator graph in the editor
    /// The UI building here is messy at best since I did not want to spend too much time on it
    /// </summary>
    internal class TransformAnimationWindow : GraphEditorBase
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

        [SerializeField] private Action _unregisterCbs;
        [SerializeField] private TransformAnimation _targetAnim;
        [SerializeField] private PropertySelectorElement _selector;
        [SerializeField] private VisualElement _parent;

        [MenuItem("Window/Functional Animation/Transform Animation Graph")]
        public static void Init()
        {
            var window = GetWindow<TransformAnimationWindow>();
            window.titleContent = new GUIContent("Transform Animation");
            window.Show();
        }

        protected override void OnAfterCreateGUI()
        {   
            _parent = new VisualElement();

            // New Menu
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
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
            _parent.Add(buttonContainer);
            _parent.Add(objField);
            _parent.Add(_selector);
            _renderElement.SetLegend(_legendMapping);

            _sideBar.AddElement("Animator", _parent);
            _sideBar.Select(1);

            // Register callbacks and set a fallback method for unbinds
            RegisterCallbacks();
            _unregisterCbs = UnRegisterCallbacks;

            void ChangeTargetAnimation(ChangeEvent<UnityEngine.Object> e)
            {
                _targetAnim = e.newValue as TransformAnimation;
                _selector.OverrideTargetContainer(_targetAnim);
                SetDrawProperty(_selector.CurrentSelection);
            }
            void CreateNewAnimation()
            {
                var path = EditorUtility.SaveFilePanelInProject("Create animation", "New Animation", "asset", "Create animation");
                
                if (string.IsNullOrEmpty(path))
                    return;
                _targetAnim = TransformAnimation.CreateNew(path);
                objField.value = _targetAnim;
                _selector.OverrideTargetContainer(_targetAnim);
                SetDrawProperty(_selector.CurrentSelection);
            }

            void SaveAsNew()
            {
                var path = EditorUtility.SaveFilePanelInProject("Save animation", "New Animation", "asset", "Save animation");

                if(string.IsNullOrEmpty(path)) 
                    return;
                if (_targetAnim != null)
                {
                    AssetDatabase.SaveAssetIfDirty(_targetAnim);
                    _targetAnim = TransformAnimation.SaveAsNew(path, _targetAnim);
                }
                else
                    _targetAnim = TransformAnimation.CreateNew(path);

                objField.value = _targetAnim;
                _selector.OverrideTargetContainer(_targetAnim);
                SetDrawProperty(_selector.CurrentSelection);
            }

            void RegisterCallbacks()
            {
                objField.RegisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked += CreateNewAnimation;
                _selector.OnSelectionChanged += SetDrawProperty;
                _renderElement.OnGraphModified += SetDirty;
                _selector.OnPropertyChanged += SetDirty;
                _selector.OnDrawElementsChanged += UpdateDrawAxis;
                _selector.OnFunctionRemoved += _renderElement.SetNodeMarkers;
                saveAsButton.clicked += SaveAsNew;
            }

            void UnRegisterCallbacks()
            {
                objField.UnregisterValueChangedCallback(ChangeTargetAnimation);
                createNewButton.clicked -= CreateNewAnimation;
                _selector.OnSelectionChanged -= SetDrawProperty;
                _renderElement.OnGraphModified -= SetDirty;
                _selector.OnPropertyChanged -= SetDirty;
                _selector.OnDrawElementsChanged -= UpdateDrawAxis;
                _selector.OnFunctionRemoved -= _renderElement.SetNodeMarkers;
                saveAsButton.clicked -= SaveAsNew;
            }

            void UpdateDrawAxis(bool4 axis, TransformProperty prop)
            {
                if (axis[3])
                {
                    for (int i = 0; i < 3; i++)
                        _renderElement.DisableDrawTarget(i);
                    _renderElement.SetDrawTarget(3, _targetAnim[prop][Axis.W]);
                }
                else
                {
                    _renderElement.DisableDrawTarget(3);
                    for(int i = 0; i < 3; ++i)
                    {
                        if (axis[i])
                            _renderElement.SetDrawTarget(i, _targetAnim[prop][i]);
                        else
                            _renderElement.DisableDrawTarget(i);
                    }
                }
            }

            void SetDirty()
            {
                if(_targetAnim != null)
                   EditorUtility.SetDirty(_targetAnim);
            }

            void SetDrawProperty(TransformProperty prop)
            {
                if (_targetAnim == null)
                {
                    _renderElement.SetDrawTargets(0, null);
                    return;
                }
                var container = _targetAnim[prop];
                var array = new GraphData[4];
                if (!container.Animate)
                    _renderElement.SetDrawTargets(0, array);
                else if(!container.SeparateAxis)
                {
                    array[3] = container[Axis.W];
                    _renderElement.SetDrawTargets(3, array);
                }
                else
                {
                    for(int i = 0; i < 3; ++i)
                    {
                        if (container.AnimateableAxis[i])
                            array[i] = container[i];
                    }
                    _renderElement.SetDrawTargets((int)_selector.CurrentAxis, array);
                }

                _renderElement.SetLegendHeader(prop.ToString());
                _renderElement.SetNodeMarkers();
            }
        }

        protected override void AddFunction(FunctionPosition pos)
        {
            if (_targetAnim == null)
                return;

            var realPos = _renderElement.GetGraphPosition(pos.Position);
            var container = _targetAnim[_selector.CurrentSelection];
            container.AddFunction(pos.Function, _selector.CurrentAxis, realPos);
            _renderElement.SetNodeMarkers();
            _selector.AxisController.Refresh();
        }

        protected override void OnDisable()
        {   
            base.OnDisable();
            if(_targetAnim != null)
                AssetDatabase.SaveAssetIfDirty(_targetAnim);
            _unregisterCbs?.Invoke();
        }        
    }
}

