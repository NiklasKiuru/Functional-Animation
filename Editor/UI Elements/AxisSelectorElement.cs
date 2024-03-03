using Aikom.FunctionalAnimation.Utility;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Aikom.FunctionalAnimation.Extensions;
using System;

namespace Aikom.FunctionalAnimation.Editor
{
    public class AxisSelectorElement : HorizontalGroupElement<GraphData, Axis>, IGraphController
    {
        public AxisSelectorElement(ICustomIndexable<GraphData, Axis> container, Dictionary<string, Axis> elementMapping) : 
            base(container, elementMapping)
        {
        }

        public event Action OnFunctionRemoved;

        public GraphData GetSource()
        {
            return _container[CurrentSelection];
        }

        public void Refresh()
        {
            OnFunctionRemoved?.Invoke();
            OverrideTargetContainer(_container);
        }

        protected override VisualElement CreateDisplayContainer(Axis selection)
        {
            var element = new VisualElement();
            if(_container == null)
                return element;
            var data = _container[selection];
            var header = UIExtensions.CreateElement<Label>(element);
            header.text = "Change or remove functions";
            header.style.flexDirection = FlexDirection.Row;
            header.style.marginBottom = new StyleLength(new Length(10f, LengthUnit.Pixel));

            int index = 0;
            foreach(var dataPoint in data.Functions)
            {
                var selector = new GraphFunctionController<Axis>.FunctionSelectionField(index.ToString() + ".", dataPoint);
                selector.Index = index;
                selector.Parent = this;
                selector.RegisterValueChangedCallback(ChangeFunction);
                selector.OnFunctionRemovedInUI += Refresh;
                element.Add(selector);
                index++;
            }

            void ChangeFunction(ChangeEvent<Enum> evt)
            {
                var target = evt.target as GraphFunctionController<Axis>.FunctionSelectionField;
                data.ChangeFunction(target.Index, (Function)evt.newValue);
            }
            return element;
        }
    }
}
