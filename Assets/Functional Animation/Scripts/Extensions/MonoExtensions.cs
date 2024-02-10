using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Animator = Aikom.FunctionalAnimation.EFAnimator;
using System.Reflection;
using UnityEngine.LowLevel;

namespace Aikom.FunctionalAnimation.Extensions
{
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
            var data = new VectorInterpolator
            {
                //Property = prop,
                //Start = prop.GetValue(tr),
                //End = target,
                Clock = new Clock(duration, TimeControl.Loop)
            };
            return default;
        }

        public static IInterpolatorHandle<float> ModulateIntensity(this Light light, Function func, float duration, float target, TimeControl ctrl)
        {   
            var data = new FloatInterpolator
            {
                Length = 1,
                Start = light.intensity,
                End = target,
                Clock = new Clock(duration, ctrl)
            };
            var function = new RangedFunction(func);
            return Animator.RegisterFloatTarget(data, function);
        }

        public static IInterpolatorHandle<float> ModulateIntensity(this Light light, GraphData graph, float duration, float target, TimeControl ctrl)
        {   
            var funcs = graph.GetRangedFunctionArray();
            var data = new FloatInterpolator
            {
                Length = funcs.Length,
                Start = light.intensity,
                End = target,
                Clock = new Clock(duration, ctrl)
            };
            return Animator.RegisterFloatTarget(data, funcs);
        }
    }

}
