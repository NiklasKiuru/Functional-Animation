using Aikom.FunctionalAnimation.Extensions;
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

        protected override Vector3 IncrimentValue(float time, Vector3 startval, Vector3 endVal)
        {
            return UnityExtensions.InterpolateAxis(_easingFunc, startval, endVal, Axis, time);
        }

        internal void SetNewTarget(Vector3 target) => Target = target;
    }
}
