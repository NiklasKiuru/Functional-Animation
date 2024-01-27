using Aikom.FunctionalAnimation.Utility;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.UI
{
    public class GraphDebugWindow : VisualElement
    {
        private List<FunctionSelectionField> _fields = new List<FunctionSelectionField>();
        private IIndexable<GraphData> _source;
        private EnumField _selector;
        private int _currentAxis;

        public Axis CurrentAxis { get => (Axis)_currentAxis; }
        public IIndexable<GraphData> Source { get => _source; }

        public event Action<Axis> CurrentAxisChanged;

        public GraphDebugWindow(IIndexable<GraphData> source, Enum usedType, int defaultIndex)
        {   
            _currentAxis = defaultIndex;
            _source = source;
            style.flexDirection = FlexDirection.Column;
            style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));

            var debugHeader = new Label("Debug");
            _selector = new EnumField("Selected Axis", Axis.X);
            _selector.RegisterValueChangedCallback(UpdateWindow);

            Add(debugHeader);
            Add(_selector);

            for (int i = 0; i < source[defaultIndex].Functions.Length; i++)
            {    
                var field = new FunctionSelectionField(i.ToString(), usedType);
                field.value = source[defaultIndex].Functions[i];
                field.Index = i;
                field.Parent = this;
                _fields.Add(field);
                Add(field);
            }
        }

        ~GraphDebugWindow()
        {
            _selector.UnregisterValueChangedCallback(UpdateWindow);
        }

        public void UpdateWindow(ChangeEvent<Enum> evt)
        {
            _currentAxis = (int)(Axis)evt.newValue;
            CurrentAxisChanged?.Invoke(CurrentAxis);
            Refresh();
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

        public void OverrideTargetContainer(IIndexable<GraphData> source)
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
            public GraphDebugWindow Parent { get; set; }

            public static event Action<Axis> OnFunctionRemovedInUI;

            public FunctionSelectionField(string label, Enum defaultValue) : base(label, defaultValue)
            {   
                style.flexDirection = FlexDirection.Row;
                RemoveButton = new Button(OnRemoveButtonClicked);
                RemoveButton.text = "Remove";
                Add(RemoveButton);
            }

            public void OnRemoveButtonClicked()
            {
                Parent.Source[(int)Parent.CurrentAxis].RemoveFunction(Index);
                OnFunctionRemovedInUI?.Invoke(Parent.CurrentAxis);
                Parent.Refresh();
            }
        }
    }
}

