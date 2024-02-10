using Aikom.FunctionalAnimation.Utility;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation.Editor
{
    public class PropertySelectorElement : HorizontalGroupElement<AnimationData, TransformProperty>
    {   
        private AxisSelectorElement _axisSelector;
        private readonly Dictionary<string, Axis> _axisMapping = new Dictionary<string, Axis>
        {
            { "X", Axis.X },
            { "Y", Axis.Y },
            { "Z", Axis.Z },
            { "All", Axis.W }
        };

        public Axis CurrentAxis { get => _axisSelector.CurrentSelection; }
        public IGraphController AxisController { get => _axisSelector; }

        public PropertySelectorElement(ICustomIndexable<AnimationData, TransformProperty> container, 
            Dictionary<string, TransformProperty> elementMapping) : 
            base(container, elementMapping)
        {
            
        }

        protected override VisualElement CreateDisplayContainer(TransformProperty selection)
        {   
            var element = new VisualElement();
            if(_container == null)
                return element;
            var data = _container[selection];
            var animateToggle = new Toggle("Animate");
            animateToggle.value = data.Animate;
            var axisToggle = new Toggle("Separate Axis");
            axisToggle.value = data.SeparateAxis;
            var durationField = new FloatField("Duration");
            durationField.value = data.Duration;
            var timeCtrlField = new EnumField("Time Control", data.TimeControl);
            var modeCtrlField = new EnumField("Animation Mode", data.Mode);
            var offsetField = new Vector3Field("Offset");
            offsetField.value = data.Offset;
            var startField = new Vector3Field("Start");
            startField.value = data.Start;
            var endField = new Vector3Field("End");
            endField.value = data.Target;

            if(data.Mode == AnimationMode.Relative)
            {
                startField.SetEnabled(false);
                endField.SetEnabled(false);
            }
            else
                offsetField.SetEnabled(false);



            _axisSelector = new AxisSelectorElement(_container[TransformProperty.Position], _axisMapping);
            _axisSelector.OverrideTargetContainer(data);
            foreach (var subElement in _axisSelector.SubSelectionElements)
            {
                var perAxisToggle = new Toggle();
                perAxisToggle.style.marginBottom = 1;
                var axis = subElement.Value;
                perAxisToggle.RegisterValueChangedCallback(evt => data.AnimateableAxis[(int)axis] = evt.newValue);
                if (axis != Axis.W)
                {
                    perAxisToggle.value = data.AnimateableAxis[(int)subElement.Value];
                    subElement.Add(perAxisToggle);
                    subElement.SetEnabled(data.SeparateAxis);
                }
                else
                    subElement.SetEnabled(!data.SeparateAxis);
            }

            element.Add(animateToggle);
            element.Add(axisToggle);
            element.Add(durationField);
            element.Add(timeCtrlField);
            element.Add(modeCtrlField);
            element.Add(offsetField);
            element.Add(startField);
            element.Add(endField);
            element.Add(_axisSelector);

            // Callbacks
            void OnAnimateToggleChange(ChangeEvent<bool> evt) => data.Animate = evt.newValue;
            void OnDurationFieldChange(ChangeEvent<float> evt) => data.Duration = evt.newValue;
            void OnTimeCtrlFieldChange(ChangeEvent<Enum> evt) => data.TimeControl = (TimeControl)evt.newValue;
            void OnOffsetFieldChange(ChangeEvent<Vector3> evt) => data.Offset = evt.newValue;
            void OnStartFieldChange(ChangeEvent<Vector3> evt) => data.Start = evt.newValue;
            void OnEndFieldChange(ChangeEvent<Vector3> evt) => data.Target = evt.newValue;

            void OnModeCtrlFieldChange(ChangeEvent<Enum> evt)
            {
                data.Mode = (AnimationMode)evt.newValue;
                if(data.Mode == AnimationMode.Relative)
                {
                    startField.SetEnabled(false);
                    endField.SetEnabled(false);
                    offsetField.SetEnabled(true);
                }
                else
                {
                    startField.SetEnabled(true);
                    endField.SetEnabled(true);
                    offsetField.SetEnabled(false);
                }
            }

            void OnAxisToggleChange(ChangeEvent<bool> evt)
            {
                data.SeparateAxis = evt.newValue;
                if (data.SeparateAxis)
                {
                    _axisSelector.UnlockSelections(Axis.X);
                    _axisSelector.DisableSelection(Axis.W);
                }
                else
                {
                    _axisSelector.LockSelection(Axis.W);
                }
            }

            animateToggle.RegisterValueChangedCallback(OnAnimateToggleChange);
            durationField.RegisterValueChangedCallback(OnDurationFieldChange);
            timeCtrlField.RegisterValueChangedCallback(OnTimeCtrlFieldChange);
            offsetField.RegisterValueChangedCallback(OnOffsetFieldChange);
            startField.RegisterValueChangedCallback(OnStartFieldChange);
            endField.RegisterValueChangedCallback(OnEndFieldChange);
            modeCtrlField.RegisterValueChangedCallback(OnModeCtrlFieldChange);
            axisToggle.RegisterValueChangedCallback(OnAxisToggleChange);

            return element;
        }
    }
}
