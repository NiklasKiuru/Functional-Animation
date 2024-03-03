using Aikom.FunctionalAnimation.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aikom.FunctionalAnimation.Editor
{
    public class HorizontalGroupController : VisualElement
    {
        private List<SubSelectionElement<int>> _clickElements = new();
        private List<VisualElement> _displayElements = new();
        private VisualElement _headerElement;
        private VisualElement _currentDisplayElement;

        public HorizontalGroupController() 
        {   
            _headerElement = UIExtensions.CreateElement(this);
            _headerElement.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            _headerElement.style.minHeight = 20;
            _headerElement.style.flexDirection = FlexDirection.Row;
            _headerElement.style.borderBottomWidth = 2;
            _headerElement.style.borderTopWidth = 5;
            _headerElement.style.paddingBottom = 3;
            _headerElement.style.borderBottomColor = new Color(0.8f, 0.8f, 0.8f);
            Add(_headerElement);
        }

        public void AddElement(string selector, VisualElement displayElement)
        {   
            var element = new SubSelectionElement<int>(_clickElements.Count, selector);
            element.RegisterCallback<MouseUpEvent>(OnElementSelected);
            element.LabelElement.RegisterCallback<MouseUpEvent>(OnElementSelected);
            _clickElements.Add(element);
            _displayElements.Add(displayElement);
            _headerElement.Add(element);
        }

        public void Select(int index)
        {
            if (_currentDisplayElement != null)
                Remove(_currentDisplayElement);
            for (int i = 0; i < _clickElements.Count; i++)
            {
                if (_clickElements[i].Value == index)
                {
                    var newElement = _displayElements[index];
                    _currentDisplayElement = newElement;
                    _clickElements[i].style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
                    Add(newElement);
                }
                else
                    _clickElements[i].style.backgroundColor = Color.clear;
            }
        }

        private void OnElementSelected(MouseUpEvent evt)
        {
            SubSelectionElement<int> element;
            var target = evt.target;
            if(target as Label == null)
                element = evt.target as SubSelectionElement<int>;
            else
            {
                var label = target as Label;
                element = label.parent as SubSelectionElement<int>;
            }

            Select(element.Value);
        }
    }
}

