using Aikom.FunctionalAnimation.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class GraphEditorWindow : GraphEditorBase, IGraphController
    {
        [SerializeField] private VisualElement _selector;
        [SerializeField] private VisualElement _parent;

        public event Action OnFunctionRemoved;

        public GraphData GraphData { get; private set; }
        public SerializedProperty GraphProperty { get; private set; }
        public SerializedObject SerializedObject { get; private set; }

        [MenuItem("Window/Functional Animation/Graph Editor")]
        public static void Init()
        {
            var window = GetWindow<GraphEditorWindow>();
            window.titleContent = new GUIContent("Graph Editor");
            window.Show();
        }

        protected override void OnAfterCreateGUI()
        {
            _parent = new VisualElement();
            _sideBar.AddElement("Graph", _parent);
            _sideBar.Select(1);
            _renderElement.OnGraphModified += ApplyModifications;
        }

        protected override void AddFunction(FunctionPosition pos)
        {
            if (GraphData == null)
                return;

            var realPos = _renderElement.GetGraphPosition(pos.Position);
            
            GraphData.AddFunction(pos.Function, realPos);
            _renderElement.SetNodeMarkers();
            Refresh();
            ApplyModifications();
        }

        private void CreateSelector()
        {   
            if(GraphData == null)
                return;
            OnFunctionRemoved?.Invoke();
            _selector = new VisualElement();
            var selectorheader = UIExtensions.CreateElement<Label>(_selector);
            selectorheader.text = "Change or remove functions";
            selectorheader.style.flexDirection = FlexDirection.Row;
            selectorheader.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));
            selectorheader.style.borderTopWidth = 5;

            int index = 0;
            foreach (var dataPoint in GraphData.Functions)
            {
                var subSelector = new GraphFunctionController<Axis>.FunctionSelectionField(index.ToString() + ".", dataPoint);
                subSelector.Index = index;
                subSelector.Parent = this;
                subSelector.RegisterValueChangedCallback(ChangeFunction);
                subSelector.OnFunctionRemovedInUI += OnRemove;
                _selector.Add(subSelector);
                index++;
            }
            _parent.Add(_selector);

            void OnRemove()
            {
                ApplyModifications();
                Refresh();
            }

            void ChangeFunction(ChangeEvent<Enum> evt)
            {
                var target = evt.target as GraphFunctionController<Axis>.FunctionSelectionField;
                GraphData.ChangeFunction(target.Index, (Function)evt.newValue);
                ApplyModifications();
            }
        }

        public void SetData(GraphData data, SerializedProperty prop)
        {
            if (data == null || data.Length == 0)
                return;

            GraphData = data;
            GraphProperty = prop;
            SerializedObject = prop.serializedObject;
            _renderElement.SetDrawTargets(0, GraphData);
            _renderElement.SetLegendHeader(prop.serializedObject.targetObject.name + " " + prop.name);
            Refresh();
        }

        public GraphData GetSource() => GraphData;
                                    

        private void ApplyModifications()
        {
            if (GraphProperty != null)
            {
                GraphProperty.SetValue(GraphData);
                EditorUtility.SetDirty(SerializedObject.targetObject);
            } 
        }

        public void Refresh()
        {   
            if(_selector != null)
                _parent.Remove(_selector);
            _renderElement.SetNodeMarkers();
            CreateSelector();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _renderElement.OnGraphModified -= ApplyModifications;
        }

        private void OnSelectionChange()
        {
            GraphData = null;
            GraphProperty = null;
            _renderElement.SetDrawTargets(0, null);
            _renderElement.SetLegendHeader("");
            _renderElement.SetNodeMarkers();
        }
    }
}

