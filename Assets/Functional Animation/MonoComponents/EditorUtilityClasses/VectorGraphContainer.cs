using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class VectorGraphContainer : VectorContainerBase
    {
        [Tooltip("Curve that describes the fluidity of the change in property value")]
        public AnimationCurve Curve;

        protected override Func<float, float> SetEasingFunction() => Curve.Evaluate;
    }
}
