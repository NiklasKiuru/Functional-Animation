using UnityEngine;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation.Extensions
{   
    /// <summary>
    /// Extensions for existing unity objects
    /// </summary>
    public static class MonoExtensions
    {   
        /// <summary>
        /// Makes a transition from the current value of the property to the target value
        /// </summary>
        /// <param name="component"></param>
        /// <param name="prop"></param>
        /// <param name="func"></param>
        /// <param name="duration"></param>
        /// <param name="target"></param>
        public static IInterpolatorHandle<float3> Transition(this Transform tr, TransformProperty prop, Function func, float duration, Vector3 target)
        {
            return MakeTransition(tr, prop, func, duration, target, TimeControl.PlayOnce);
        }

        /// <summary>
        /// Makes a looping transition from the current value of the property to the target value
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="prop"></param>
        /// <param name="func"></param>
        /// <param name="duration"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> TransitionLoop(this Transform tr, TransformProperty prop, Function func, float duration, Vector3 target)
        {
            return MakeTransition(tr, prop, func, duration, target, TimeControl.Loop);
        }

        /// <summary>
        /// Makes a looping transition from the current value of the property to the target value
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="prop"></param>
        /// <param name="func"></param>
        /// <param name="duration"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> TransitionPingPong(this Transform tr, TransformProperty prop, Function func, float duration, Vector3 target)
        {
            return MakeTransition(tr, prop, func, duration, target, TimeControl.PingPong);
        }

        /// <summary>
        /// Creates a transition from current property value to current + offset
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="prop"></param>
        /// <param name="func"></param>
        /// <param name="duration"></param>
        /// <param name="offset"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> TransitionByOffset(this Transform tr, TransformProperty prop, Function func, float duration, Vector3 offset, TimeControl ctrl)
        {
            var start = prop.GetValue(tr);
            var end = start + offset;
            return EF.Create(start, end, duration, func, ctrl)
                .OnUpdate(tr, (v) => prop.SetValue(tr, v));
        }

        /// <summary>
        /// Creates an axis aligned orbit with current position as its center point
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="duration"></param>
        /// <param name="radius"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> OrbitXY(this Transform tr, float duration, float radius, int direction)
        {
            var start = tr.position;
            var sign = Mathf.Sign(direction);
            var end = start + new Vector3(sign * radius, radius, 0);
            var cont = new FunctionContainer(3);

            // x - axis
            cont.Set(0, 0, new RangedFunction(Function.EaseOutSine, new float2(0, 0), new float2(0.25f, 1)));
            cont.Set(0, 1, new RangedFunction(Function.EaseInOutSine, new float2(0.25f, 1), new float2(0.75f, -1)));
            cont.Set(0, 2, new RangedFunction(Function.EaseInSine, new float2(0.75f, -1), new float2(1, 0)));

            // y - axis
            cont.Set(1, 0, new RangedFunction(Function.EaseInOutSine, new float2(0, -1), new float2(0.5f, 1)));
            cont.Set(1, 1, new RangedFunction(Function.EaseInOutSine, new float2(0.5f, 1), new float2(1, -1)));
            var processor = EF.CreateBasic(start, end, duration, TimeControl.Loop, new int3(3, 2, 0), new bool3(true, true, false));

            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, cont);
        }

        /// <summary>
        /// Creates an axis aligned orbit with current position as its center point
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="duration"></param>
        /// <param name="radius"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> OrbitZY(this Transform tr, float duration, float radius, int direction)
        {
            var start = tr.position;
            var sign = Mathf.Sign(direction);
            var end = start + new Vector3(0, radius, sign * radius);
            var cont = new FunctionContainer(3);

            // z - axis
            cont.Set(2, 0, new RangedFunction(Function.EaseOutSine, new float2(0, 0), new float2(0.25f, 1)));
            cont.Set(2, 1, new RangedFunction(Function.EaseInOutSine, new float2(0.25f, 1), new float2(0.75f, -1)));
            cont.Set(2, 2, new RangedFunction(Function.EaseInSine, new float2(0.75f, -1), new float2(1, 0)));

            // y - axis
            cont.Set(1, 0, new RangedFunction(Function.EaseInOutSine, new float2(0, -1), new float2(0.5f, 1)));
            cont.Set(1, 1, new RangedFunction(Function.EaseInOutSine, new float2(0.5f, 1), new float2(1, -1)));
            var processor = EF.CreateBasic(start, end, duration, TimeControl.Loop, new int3(0, 2, 3), new bool3(false, true, true));

            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, cont);
        }

        /// <summary>
        /// Creates an axis aligned orbit with current position as its center point
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="duration"></param>
        /// <param name="radius"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> OrbitXZ(this Transform tr, float duration, float radius, int direction)
        {
            var start = tr.position;
            var sign = Mathf.Sign(direction);
            var end = start + new Vector3(sign * radius, 0, radius);
            var cont = new FunctionContainer(3);

            // x - axis
            cont.Set(0, 0, new RangedFunction(Function.EaseOutSine, new float2(0, 0), new float2(0.25f, 1)));
            cont.Set(0, 1, new RangedFunction(Function.EaseInOutSine, new float2(0.25f, 1), new float2(0.75f, -1)));
            cont.Set(0, 2, new RangedFunction(Function.EaseInSine, new float2(0.75f, -1), new float2(1, 0)));

            // z - axis
            cont.Set(2, 0, new RangedFunction(Function.EaseInOutSine, new float2(0, -1), new float2(0.5f, 1)));
            cont.Set(2, 1, new RangedFunction(Function.EaseInOutSine, new float2(0.5f, 1), new float2(1, -1)));
            var processor = EF.CreateBasic(start, end, duration, TimeControl.Loop, new int3(3, 0, 2), new bool3(true, false, true));

            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, cont);
        }

        /// <summary>
        /// Modulates the intensity of the light from current to target
        /// </summary>
        /// <param name="light"></param>
        /// <param name="func"></param>
        /// <param name="duration"></param>
        /// <param name="target"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float> ModulateIntensity(this Light light, Function func, float duration, float target, TimeControl ctrl)
        {
            return EF.Create(light.intensity, target, duration, func, ctrl)
                .OnUpdate(light, (v) => light.intensity = v);
        }

        /// <summary>
        /// Modulates the intensity of the light from current to target using a graph
        /// </summary>
        /// <param name="light"></param>
        /// <param name="graph"></param>
        /// <param name="duration"></param>
        /// <param name="target"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float> ModulateIntensity(this Light light, GraphData graph, float duration, float target, TimeControl ctrl)
        {
            return EF.Create(light.intensity, target, duration, graph, ctrl)
                .OnUpdate(light, (v) => light.intensity = v);
        }

        #region Helpers
        private static IInterpolatorHandle<float3> MakeTransition(Transform tr, TransformProperty prop, Function func, float duration, Vector3 target, TimeControl ctrl)
        {
            var current = prop.GetValue(tr);
            return EF.Create(current, target, duration, func, ctrl)
                .OnUpdate(tr, (v) => prop.SetValue(tr, v));
        }
        #endregion
    }

}
