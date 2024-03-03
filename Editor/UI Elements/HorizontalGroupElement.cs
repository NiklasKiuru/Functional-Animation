using Aikom.FunctionalAnimation.Extensions;
using Aikom.FunctionalAnimation.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace Aikom.FunctionalAnimation.Editor
{
    public abstract class HorizontalGroupElement<T, D> : VisualElement where T : class
    {
        protected ICustomIndexable<T, D> _container;
        protected Dictionary<string, D> _elementMapping;
        protected Dictionary<D, SubSelectionElement<D>> _subElements = new();
        protected D _currentSelection;
        protected VisualElement _mainContainer;
        protected VisualElement _selectorParent;

        /// <summary>
        /// List of all sub selector elements
        /// </summary>
        public List<SubSelectionElement<D>> SubSelectionElements { get => _subElements.Values.ToList(); }

        /// <summary>
        /// Currently assigned selection
        /// </summary>
        public D CurrentSelection { get => _currentSelection; }

        /// <summary>
        /// Fired when the current selection is changed in the UI
        /// </summary>
        public event Action<D> OnSelectionChanged;

        public HorizontalGroupElement(ICustomIndexable<T, D> container, Dictionary<string, D> elementMapping)
        {
            _container = container;
            _elementMapping = elementMapping;
            _selectorParent = UIExtensions.CreateElement(this);
            _selectorParent.style.flexDirection = FlexDirection.Row;

            foreach (var kvp in elementMapping)
            {
                var subElement = new SubSelectionElement<D>(kvp.Value, kvp.Key);
                subElement.LabelElement.RegisterCallback<MouseDownEvent>(OnMouseSelect);
                _subElements.Add(kvp.Value, subElement);
                _selectorParent.Add(subElement);
            }
            SelectProperty(_subElements.Values.First());
        }

        /// <summary>
        /// Overrides target container with the given source
        /// </summary>
        /// <param name="source"></param>
        public void OverrideTargetContainer(ICustomIndexable<T, D> source)
        {
            _container = source;
            ChangeMainDisplay();
        }

        /// <summary>
        /// Locks the selection to the given value
        /// </summary>
        /// <param name="selection"></param>
        public void LockSelection(D selection)
        {
            if (_subElements.ContainsKey(selection))
            {
                SelectProperty(_subElements[selection]);
                foreach (var kvp in _subElements)
                {
                    kvp.Value.SetEnabled(kvp.Key.Equals(selection));
                }
            }
        }

        /// <summary>
        /// Unlocks all selections and selects the given value
        /// </summary>
        /// <param name="selection"></param>
        public void UnlockSelections(D selection)
        {
            foreach (var kvp in _subElements)
            {
                kvp.Value.SetEnabled(true);
            }
            SelectProperty(_subElements[selection]);
        }

        /// <summary>
        /// Disables the given selection
        /// </summary>
        /// <param name="selection"></param>
        public void DisableSelection(D selection)
        {
            if (_subElements.ContainsKey(selection))
            {
                _subElements[selection].SetEnabled(false);
            }
        }

        private void OnMouseSelect(MouseDownEvent evt)
        {
            if(evt.button == 0)
            {
                var element = evt.target as Label;
                var subElement = element?.parent as SubSelectionElement<D>;
                
                if(subElement != null)
                {
                    SelectProperty(subElement);
                }
            }
        }

        private void SelectProperty(SubSelectionElement<D> selection)
        {
            selection.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
            var key = selection.Value;
            if (!_currentSelection.Equals(key))
            {
                _currentSelection = key;
                ChangeMainDisplay();
                OnSelectionChanged?.Invoke(_currentSelection);
            }
            foreach (var kvp in _subElements)
            {
                if (!kvp.Key.Equals(key))
                {
                    kvp.Value.style.backgroundColor = Color.clear;
                }
            }
        }

        private void ChangeMainDisplay()
        {   
            if(_mainContainer != null)
                Remove(_mainContainer);
            _mainContainer = CreateDisplayContainer(_currentSelection);
            _mainContainer.style.flexGrow = 1;

            _mainContainer.style.flexGrow = 1;
            _mainContainer.style.marginBottom = 5;
            //_mainContainer.style.marginTop = 5;
            _mainContainer.style.marginLeft = 5;
            _mainContainer.style.marginRight = 5;

            _mainContainer.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);
            _mainContainer.style.borderBottomWidth = 1;
            _mainContainer.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
            _mainContainer.style.borderTopWidth = 1;
            _mainContainer.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f);
            _mainContainer.style.borderLeftWidth = 1;
            _mainContainer.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f);
            _mainContainer.style.borderRightWidth = 1;

            _mainContainer.style.paddingBottom = 5;
            _mainContainer.style.paddingTop = 5;
            _mainContainer.style.paddingLeft = 5;
            _mainContainer.style.paddingRight = 5;

            _mainContainer.style.minHeight = 100;

            Add(_mainContainer);
        }

        protected abstract VisualElement CreateDisplayContainer(D selection);
    }
}

