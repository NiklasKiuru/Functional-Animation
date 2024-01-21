using Aikom.FunctionalAnimation.Extensions;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public abstract class VectorContainerBase : PropertyContainer<Vector3>
    {
        [Tooltip("The offset value that is added to current property")]
        public Vector3 Offset;

        [Tooltip("You can include or exclude separate axis from the animation")]
        public bool3 Axis = new bool3(true);

        protected override Vector3 IncrimentValue(Vector3 startVal, float time, Func<float, float> easingFunc)
        {
            return UnityExtensions.InterpolateAxis(easingFunc, startVal, Target, Axis, time);
        }

        internal void SetNewTarget(Vector3 target) => Target = target;
    }
}
