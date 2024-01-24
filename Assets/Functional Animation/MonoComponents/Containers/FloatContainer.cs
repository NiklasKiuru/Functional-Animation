using System;
using UnityEngine.Events;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class FloatContainer : PropertyContainer<float>
    {
        [Tooltip("Starting value of the animation")]
        public float StartValue;

        [Tooltip("Easing function")]
        public FunctionConstructor FunctionConstructor = new();

        [Tooltip("Target callback event")]
        public UnityEvent<float> OnValueChangedEvent = new();

        protected override Func<float, float> GenerateEasingFunction()
        {
            FunctionConstructor ??= new FunctionConstructor();
            return FunctionConstructor.Generate();
        }

        internal override float IncrimentValue(float time, float start, float end)
        {
            return EF.Interpolate(_easingFunc, start, end, time);
        }
    }
}
