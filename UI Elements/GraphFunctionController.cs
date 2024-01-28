using Aikom.FunctionalAnimation.Utility;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.UI
{
    public class GraphFunctionController : VisualElement
    {
        private List<FunctionSelectionField> _fields = new List<FunctionSelectionField>();
        private IIndexable<GraphData, Axis> _source;
        private EnumField _selector;
        private Axis _currentAxis;

        public Axis CurrentAxis { get => _currentAxis; }
        public IIndexable<GraphData, Axis> Source { get => _source; }

        public event Action<Axis> CurrentAxisChanged;

        public GraphFunctionController(IIndexable<GraphData, Axis> source, Enum usedType, Axis defaultIndex)
        {   
            _currentAxis = defaultIndex;
            _source = source;
            style.flexDirection = FlexDirection.Column;
            style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));

            var debugHeader = new Label("Change or remove functions");
            debugHeader.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));
            _selector = new EnumField("Selected Axis:", Axis.X);
            _selector.RegisterValueChangedCallback(UpdateWindow);

            Add(debugHeader);
            Add(_selector);

            for (int i = 0; i < source[defaultIndex].Functions.Length; i++)
            {    
                var field = new FunctionSelectionField(i.ToString() + ".", usedType);
                field.value = source[defaultIndex].Functions[i];
                field.Index = i;
                field.Parent = this;
                _fields.Add(field);
                Add(field);
            }
        }

        ~GraphFunctionController()
        {
            _selector.UnregisterValueChangedCallback(UpdateWindow);
        }

        private void UpdateWindow(ChangeEvent<Enum> evt)
        {
            _currentAxis = (Axis)evt.newValue;
            CurrentAxisChanged?.Invoke(CurrentAxis);
            Refresh();
        }

        public void LockSelector(Axis axis)
        {   
            _currentAxis = axis;
            CurrentAxisChanged?.Invoke(CurrentAxis);
            Refresh();
            _selector.SetEnabled(false);
        }

        public void UnlockSelector(Axis defaultAxis)
        {   
            _currentAxis = defaultAxis;
            CurrentAxisChanged?.Invoke(CurrentAxis);
            Refresh();
            _selector.SetEnabled(true);
        }

        private void UnregisterCallBacks()
        {
            foreach (var field in _fields)
            {
                field.UnregisterValueChangedCallback(OnValueChanged);
                field.RemoveButton.clicked -= field.OnRemoveButtonClicked;
            }
        }

        private void RegisterCallBacks()
        {
            foreach (var field in _fields)
            {
                field.RegisterValueChangedCallback(OnValueChanged);
            }
        }

        private void OnValueChanged(ChangeEvent<Enum> evt)
        {
            var value = (Function)evt.newValue;
            var field = (FunctionSelectionField)evt.target;
            _source[_currentAxis].Functions[field.Index] = value;
        }

        public void OverrideTargetContainer(IIndexable<GraphData, Axis> source)
        {
            _source = source;
            Refresh();
        }

        public void Refresh()
        {
            UnregisterCallBacks();
            
            for (int i = 0; i < _fields.Count; i++)
            {
                Remove(_fields[i]);
            }

            _fields.Clear();
            for (int i = 0; i < _source[_currentAxis].Functions.Length; i++)
            {
                var field = new FunctionSelectionField(i.ToString(), _source[_currentAxis].Functions[i]);
                field.Index = i;
                field.Parent = this;
                _fields.Add(field);
                Add(field);
            }
            RegisterCallBacks();
        }

        public class FunctionSelectionField : EnumField
        {   
            public int Index { get; set; }
            public Button RemoveButton { get; private set; }
            public GraphFunctionController Parent { get; set; }

            public static event Action<Axis> OnFunctionRemovedInUI;

            public FunctionSelectionField(string label, Enum defaultValue) : base(label, defaultValue)
            {   
                style.flexDirection = FlexDirection.Row;
                labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                RemoveButton = new Button(OnRemoveButtonClicked);
                RemoveButton.text = "Remove";
                Add(RemoveButton);
            }

            public void OnRemoveButtonClicked()
            {   
                if(Index == 0 && Parent.Source[Parent.CurrentAxis].Functions.Length == 1)
                {
                    Debug.LogWarning("Cannot remove the only function in the array");
                    return;
                }
                Parent.Source[Parent.CurrentAxis].RemoveFunction(Index);
                OnFunctionRemovedInUI?.Invoke(Parent.CurrentAxis);
                Parent.Refresh();
            }
        }
    }
}

