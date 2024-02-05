using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Animator = Aikom.FunctionalAnimation.Animator;

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
        public static void Transition(this MonoBehaviour component, TransformProperty prop, Function func, float duration, Vector3 target)
        {   
                        
        }
    }

}
