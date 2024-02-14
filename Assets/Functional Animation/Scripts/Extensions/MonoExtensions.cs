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
