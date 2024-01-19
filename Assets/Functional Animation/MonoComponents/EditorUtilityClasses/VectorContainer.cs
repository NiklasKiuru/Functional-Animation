using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class VectorContainer : VectorContainerBase
    {
        [Tooltip("Easing function")]
        public FunctionConstructor FunctionConstructor;

        protected override Func<float, float> SetEasingFunction()
        {
            FunctionConstructor ??= new FunctionConstructor();
            return FunctionConstructor.Generate();
        }
    }
}
