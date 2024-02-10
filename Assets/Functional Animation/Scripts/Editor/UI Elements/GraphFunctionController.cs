using Aikom.FunctionalAnimation.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class GraphFunctionController<T> : VisualElement, IGraphController
    {
        private List<FunctionSelectionField> _fields = new List<FunctionSelectionField>();
        private ICustomIndexable<GraphData, T> _source;
        private Dictionary<string, T> _selectionMapping = new Dictionary<string, T>();
        private DropdownField _selector;
        private T _currentSelection;

        /// <summary>
        /// Currently assigned selection
        /// </summary>
        public T CurrentSelection { get => _currentSelection; }

        /// <summary>
        /// Source container for the functions this element reads from
        /// </summary>
        public ICustomIndexable<GraphData, T> Source { get => _source; }

        /// <summary>
        /// Fired when the current selection is changed in the UI
        /// </summary>
        public event Action<T> CurrentSelectionChanged;

        /// <summary>
        /// Creates a new GraphFunctionController
        /// </summary>
        /// <param name="source"></param>
        /// <param name="defaultIndex"></param>
        /// <param name="defaultMapping">Tells the selector what data to show in the dropdown menu and how the options map into the indexer</param>
        public GraphFunctionController(ICustomIndexable<GraphData, T> source, T defaultIndex, Dictionary<string, T> defaultMapping)
        {   
            _currentSelection = defaultIndex;
            _source = source;
            style.flexDirection = FlexDirection.Column;
            style.marginTop = new StyleLength(new Length(10f, LengthUnit.Pixel));
            _selectionMapping = defaultMapping;

            var debugHeader = new Label("Change or remove functions");
            debugHeader.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));
            _selector = new DropdownField("Selected: ", defaultMapping.Keys.ToList(), 0);
            _selector.RegisterValueChangedCallback(UpdateWindow);
            Add(debugHeader);
            Add(_selector);
            CreateFields();
        }

        ~GraphFunctionController()
        {
            _selector.UnregisterValueChangedCallback(UpdateWindow);
        }

        public GraphData GetSource()
        {
            return _source[_currentSelection];
        }

        /// <summary>
        /// Locks the selector to the given index and removes other options from the drop down menu
        /// </summary>
        /// <param name="indexer"></param>
        public void LockSelector(T indexer)
        {   
            _currentSelection = indexer;
            _selector.UnregisterValueChangedCallback(UpdateWindow);
            _selector.RemoveFromHierarchy();
            _selector = new DropdownField("Selected: ", new List<string> { _selectionMapping.FirstOrDefault(x => x.Value.Equals(indexer)).Key }, 0);
            _selector.RegisterValueChangedCallback(UpdateWindow);
            Insert(1, _selector);
            CurrentSelectionChanged?.Invoke(CurrentSelection);
            Refresh();
        }

        /// <summary>
        /// Unlocks the selector and sets the default index to the given value
        /// </summary>
        /// <param name="defaultindex"></param>
        public void UnlockSelector(T defaultindex)
        {   
            _currentSelection = defaultindex;
            _selector.UnregisterValueChangedCallback(UpdateWindow);
            _selector.RemoveFromHierarchy();
            _selector = new DropdownField("Selected: ", _selectionMapping.Keys.ToList(), 0);
            _selector.value = _selectionMapping.FirstOrDefault(x => x.Value.Equals(defaultindex)).Key;
            Insert(1, _selector);
            _selector.RegisterValueChangedCallback(UpdateWindow);
            CurrentSelectionChanged?.Invoke(CurrentSelection);
            Refresh();
        }

        /// <summary>
        /// Overrides target container with the given source
        /// </summary>
        /// <param name="source"></param>
        public void OverrideTargetContainer(ICustomIndexable<GraphData, T> source)
        {
            _source = source;
            Refresh();
        }

        /// <summary>
        /// Refreshes the UI to match the current state of the source
        /// </summary>
        public void Refresh()
        {
            UnregisterCallBacks();
            for (int i = 0; i < _fields.Count; i++)
            {
                Remove(_fields[i]);
            }
            CreateFields();
            RegisterCallBacks();
        }

        private void CreateFields()
        {   
            _fields.Clear();
            int index = 0;
            foreach(var func in _source[_currentSelection].Functions)
            {
                var field = new FunctionSelectionField(index.ToString(), func);
                field.Index = index;
                field.Parent = this;
                field.OnFunctionRemovedInUI += Refresh;
                _fields.Add(field);
                Add(field);
                index++;
            }
        }

        private void UpdateWindow(ChangeEvent<string> evt)
        {
            _currentSelection = _selectionMapping[evt.newValue];
            CurrentSelectionChanged?.Invoke(CurrentSelection);
            Refresh();
        }

        private void UnregisterCallBacks()
        {
            foreach (var field in _fields)
            {
                field.UnregisterValueChangedCallback(OnValueChanged);
                field.RemoveButton.clicked -= field.OnRemoveButtonClicked;
                field.OnFunctionRemovedInUI -= CreateFields;
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
            _source[_currentSelection].ChangeFunction(field.Index, value);
        }

        /// <summary>
        /// Selection field for the GraphFunctionController
        /// </summary>
        public class FunctionSelectionField : EnumField 
        {   
            /// <summary>
            /// Represents the position of this field in the source container
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Removes this index from the source container once clicked
            /// </summary>
            public Button RemoveButton { get; private set; }

            /// <summary>
            /// Parent controller
            /// </summary>
            public IGraphController Parent { get; set; }

            /// <summary>
            /// Fired when the remove button is clicked
            /// </summary>
            public event Action OnFunctionRemovedInUI;

            /// <summary>
            /// Creates a new FunctionSelectionField
            /// </summary>
            /// <param name="label"></param>
            /// <param name="defaultValue"></param>
            public FunctionSelectionField(string label, Enum defaultValue) : base(label, defaultValue)
            {   
                style.flexDirection = FlexDirection.Row;
                labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
                labelElement.style.minWidth = new StyleLength(new Length(20f, LengthUnit.Pixel));
                RemoveButton = new Button(OnRemoveButtonClicked);
                RemoveButton.text = "Remove";
                Add(RemoveButton);
            }

            /// <summary>
            /// Removes a function in this field's index from the source container
            /// </summary>
            public void OnRemoveButtonClicked()
            {   
                var source = Parent.GetSource();
                if(source.RemoveFunction(Index))
                    OnFunctionRemovedInUI?.Invoke();

            }
        }
    }
}

