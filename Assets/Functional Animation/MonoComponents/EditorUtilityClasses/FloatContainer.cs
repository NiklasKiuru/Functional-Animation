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

        protected override Func<float, float> SetEasingFunction()
        {
            FunctionConstructor ??= new FunctionConstructor();
            return FunctionConstructor.Generate();
        }

        protected override float IncrimentValue(float startVal, float time, Func<float, float> easingFunc)
        {
            return EF.Interpolate(easingFunc, startVal, Target, time);
        }
    }
}
