using System;
using UnityEngine;
using System.Collections.Generic;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class VectorContainer : VectorContainerBase, ICloneable
    {
        [Tooltip("Easing function")]
        public FunctionConstructor FunctionConstructor;

        public object Clone()
        {
            var copy = new VectorContainer();
            copy.FunctionConstructor = FunctionConstructor.Clone() as FunctionConstructor;
            copy.Axis = Axis;
            copy.TrimFront = TrimFront;
            copy.TrimBack = TrimBack;
            copy.Target = Target;
            copy.Animate = Animate;
            copy.Offset = Offset;
            copy.TimeControl = TimeControl;
            copy.Duration = Duration;

            return copy;
        }

        protected override Func<float, float> GenerateEasingFunction()
        {
            FunctionConstructor ??= new FunctionConstructor();
            return FunctionConstructor.Generate();
        }
    }
}
