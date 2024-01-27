using PlasticGui.WorkspaceWindow.PendingChanges;
using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Collection of interpolation and easing functions
    /// Credits to easings.net for some of the basic easing functions
    /// </summary>
    public struct EF
    {
        #region Easing functions
        public static float EaseInSine(float x)
        {
            return 1 - Mathf.Cos((x * Mathf.PI) / 2);
        }

        public static float EaseOutSine(float x)
        {
            return Mathf.Sin((x * Mathf.PI) / 2);
        }

        public static float EaseInOutSine(float x)
        {
            return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
        }

        public static float EaseInExp(float x)
        {
            return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
        }

        public static float EaseOutExp(float x)
        {
            return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
        }

        public static float EaseInOutExp(float x)
        {
            return x == 0 ? 0 :
                x == 1 ? 1 :
                x < 0.5f ? Mathf.Pow(2, 20 * x - 10) / 2 :
                (2 - Mathf.Pow(2, -20 * x + 10)) / 2;
        }

        public static float EaseInBounce(float x)
        {
            return 1 - EaseOutBounce(1 - x);
        }

        public static float EaseOutBounce(float x)
        {
            if (x < 1 / 2.75f)
            {
                return 7.5625f * x * x;
            }
            else if (x < 2 / 2.75f)
            {
                return 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f;
            }
            else if (x < 2.5f / 2.75f)
            {
                return 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f;
            }
            else
            {
                return 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
            }
        }

        public static float EaseInOutBounce(float x)
        {
            return x < 0.5f ? (1 - EaseOutBounce(1 - 2 * x)) / 2 : (1 + EaseOutBounce(2 * x - 1)) / 2;
        }

        public static float EaseInElastic(float x)
        {
            return Mathf.Sin(13 * Mathf.PI / 2 * x) * Mathf.Pow(2, 10 * x - 10);
        }

        public static float EaseOutElastic(float x)
        {
            return Mathf.Sin(-13 * Mathf.PI / 2 * (x + 1)) * Mathf.Pow(2, -10 * x) + 1;
        }

        public static float EaseInOutElastic(float x)
        {
            return x < 0.5f ? Mathf.Sin(13 * Mathf.PI / 2 * (2 * x)) * Mathf.Pow(2, 10 * (2 * x - 1)) / 2 :
                Mathf.Sin(-13 * Mathf.PI / 2 * (2 * x - 1 + 1)) * Mathf.Pow(2, -10 * (2 * x - 1)) / 2 + 1;
        }

        public static float EaseInCirc(float x)
        {
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
        }

        public static float EaseOutCirc(float x)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
        }

        public static float EaseInOutCirc(float x)
        {
            return x < 0.5f ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;
        }

        public static float EaseInBack(float x)
        {
            return 2.70158f * x * x * x - 1.70158f * x * x;
        }

        public static float EaseOutBack(float x)
        {
            return 1 + 2.70158f * Mathf.Pow(x - 1, 3) + 1.70158f * Mathf.Pow(x - 1, 2);
        }

        public static float EaseInOutBack(float x)
        {
            return x < 0.5f ? (Mathf.Pow(2 * x, 2) * ((2.5949095f + 1) * 2 * x - 2.5949095f)) / 2 : (Mathf.Pow(2 * x - 2, 2) * ((2.5949095f + 1) * (x * 2 - 2) + 2.5949095f) + 2) / 2;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Combines multiple easing functions into one
        /// </summary>
        /// <param name="functions">The execution order of the combined functions is dependent of the input order of this array</param>
        /// <returns>If the output is used as a function in interpolation it should be noted that the time parameter in this function works as t/n where n is the amount of input functions</returns>
        public static Func<float, float> Combine(params Function[] functions)
        {
            if (functions == null || functions.Length == 0)
                throw new ArgumentException("The input array cannot be null and must contain atleast 1 member");

            var funcs = new Func<float, float>[functions.Length];
            for (int i = 0; i < functions.Length; i++)
            {
                funcs[i] = EditorFunctions.Funcs[functions[i]];
            }

            return Combine(funcs);
        }

        /// <summary>
        /// Combines multiple easing functions into one
        /// </summary>
        /// <param name="functions">The execution order of the combined functions is dependent of the input order of this array</param>
        /// <returns>If the output is used as a function in interpolation it should be noted that the time parameter in this function works as t/n where n is the amount of input functions</returns>
        /// <exception cref="ArgumentException"></exception>
        public static Func<float, float> Combine(params Func<float, float>[] functions)
        {
            if (functions == null || functions.Length == 0)
                throw new ArgumentException("The input array cannot be null and must contain atleast 1 member");

            return f => {
                float count = functions.Length;
                for (int i = 0; i < count; i++)
                {
                    // Has to be >= due to case f == 0
                    if (f >= i / count && f < (i + 1) / count)
                        return functions[i].Invoke(count * f - i);   // Clamps the actual f value again between the iterators range
                }

                // If f == 1 the last function will be invoked automatically
                return functions[(int)count - 1].Invoke(f);
            };
        }

        /// <summary>
        /// Inverts a defined easing function and returns it as a function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Func<float, float> Invert(Function function)
        {
            var func = EditorFunctions.Funcs[function];
            return f => { return 1 - func.Invoke(f); };
        }

        /// <summary>
        /// Inverts a custom easing function and returns it as a function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public static Func<float, float> Invert(Func<float, float> function)
        {
            return f => { return 1 - function.Invoke(f); };
        }

        #endregion

        #region Interpolation
        /// <summary>
        /// Interpolates between given structs with defined easing function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="easing"></param>
        /// <param name="origin">From this object</param>
        /// <param name="target">To this object</param>
        /// <param name="convertIn">Function that converts the generic type into a float array</param>
        /// <param name="convertOut">Function that converts the float array back into the object</param>
        /// <param name="time">The completion status of the interpolation int time between 0 and 1</param>
        /// <returns></returns>
        public static T Interpolate<T>(Function easing, T origin, T target, Func<T, float[]> convertIn, Func<float[], T> convertOut, float time) where T : struct
        {
            var func = EditorFunctions.Funcs[easing];
            return Interpolate(func, origin, target, convertIn, convertOut, time);
        }

        /// <summary>
        /// Interpolates between given structs with defined easing function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="easing"></param>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static T Interpolate<T, D>(Function easing, T origin, D target, float time)
            where T : struct, IInterpolateable<T>
            where D : struct, IInterpolateable<T>
        {
            var originInputs = origin.Deconstruct();
            var targetInputs = target.Deconstruct();
            if (originInputs.Length != targetInputs.Length)
                throw new Exception("Default deconstructors of IInterpolateable objects have invalid array size");

            var values = new float[originInputs.Length];
            for (int i = 0; i < originInputs.Length; i++)
            {
                values[i] = Interpolate(easing, originInputs[i], targetInputs[i], time);
            }
            return origin.ReConstruct<T>(values, origin);
        }

        /// <summary>
        /// Interpolates between start and end using a defined easing function
        /// </summary>
        /// <param name="easing"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time">Value between 0 and 1</param>
        /// <returns></returns>
        public static float Interpolate(Function easing, float start, float end, float time)
        {
            return start + (end - start) * EditorFunctions.Funcs[easing].Invoke(Mathf.Clamp01(time));
        }

        /// <summary>
        /// Interpolates between start and end using a defined easing function and loops it if time threshold is above 1
        /// </summary>
        /// <param name="easing"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public static float Interpolate(Function easing, float start, float end, ref float time, bool loop)
        {
            if (loop && time >= 1)
                time = 0;
            return Interpolate(easing, start, end, time);
        }

        /// <summary>
        /// Interpolates between start and end using a custom easing function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="easing"></param>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <param name="convertIn"></param>
        /// <param name="convertOut"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static T Interpolate<T>(Func<float, float> easing, T origin, T target, Func<T, float[]> convertIn, Func<float[], T> convertOut, float time) where T : struct
        {
            var originInputs = convertIn(origin);
            var targetInputs = convertIn(target);
            var values = new float[targetInputs.Length];
            for (int i = 0; i < targetInputs.Length; i++)
            {
                values[i] = Interpolate(easing, originInputs[i], targetInputs[i], time);
            }
            return convertOut(values);
        }

        /// <summary>
        /// Interpolates between start and end using a custom easing function
        /// </summary>
        /// <param name="func"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float Interpolate(Func<float, float> func, float start, float end, float time)
        {
            return start + (end - start) * func.Invoke(Mathf.Clamp01(time));
        }

        #endregion
    }
}