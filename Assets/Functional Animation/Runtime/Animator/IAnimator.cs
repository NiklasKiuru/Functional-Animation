using System;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Transform animator interface
    /// </summary>
    public interface IAnimator
    {   
        /// <summary>
        /// Animator control handle
        /// </summary>
        public TransformHandle Handle { get; }
    }

    public static class AnimatorExtensions
    {   
        private static TransformProperty[] s_cachedEnumArray = (TransformProperty[])Enum.GetValues(typeof(TransformProperty));

        /// <summary>
        /// Clears the previous animation and sets a new one for the animator
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="anim"></param>
        /// <param name="target"></param>
        /// <param name="update"></param>
        internal static void SetAnimation(this IAnimator animator, TransformAnimation anim, Transform target, Action<float3, TransformProperty> update)
        {
            if (animator.Handle.IsActive)
                animator.Handle.KillAll();
            var props = s_cachedEnumArray;
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                var data = anim[prop];
                if (!data.Animate)
                    continue;

                var from = data.Mode == AnimationMode.Absolute ? data.Start : prop.GetValue(target);
                var to = data.Mode == AnimationMode.Absolute ? data.Target : prop.GetValue(target) + data.Offset;
                if (!data.SeparateAxis)
                {
                    var sharedGraph = data[Axis.W];
                    var sharedHandle = EF.Create(from, to, data.Duration, sharedGraph, data.TimeControl)
                        .OnUpdate(target, SetVal);
                    animator.Handle.Set(prop, sharedHandle);
                }
                else
                {
                    var mixedHandle = EF.Create(from, to, data.Duration,
                        data[Axis.X], data[Axis.Y], data[Axis.Z], data.AnimateableAxis, data.TimeControl)
                        .OnUpdate(target, SetVal);
                    animator.Handle.Set(prop, mixedHandle);
                }

                void SetVal(float3 val)
                {
                    update?.Invoke(val, prop);
                }
            }
        }
    }
}


