using System;
using UnityEngine.Events;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

        public FloatContainer(float start, float target, float duration, Action<float> setVal, TimeControl timeControl = TimeControl.OneShot)
        {
            Target = target;
            Duration = duration;
            TimeControl = timeControl;
            _easingFunc = GenerateEasingFunction();
            CreateInterpolator(start, setVal);
        }

        protected override Func<float, float> GenerateEasingFunction()
        {
            FunctionConstructor ??= new FunctionConstructor();
            return FunctionConstructor.GenerateSimple();
        }

        internal override float IncrimentValue(float time, float start, float end)
        {
            return EF.Interpolate(_easingFunc, start, end, time);
        }
    }
}
