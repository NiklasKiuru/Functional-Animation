using System;
using Unity.Mathematics;
using UnityEngine;


namespace Aikom.FunctionalAnimation.Extensions
{   
    /// <summary>
    /// Extension methods for Unity types
    /// </summary>
    public static class UnityExtensions
    {   
        /// <summary>
        /// Interpolates a color
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Color Interpolate(Func<float, float> ease, Color start, Color end, float time)
        {   
            return new Color(
                EF.Interpolate(ease, start.r, end.r, time),
                EF.Interpolate(ease, start.g, end.g, time),
                EF.Interpolate(ease, start.b, end.b, time),
                EF.Interpolate(ease, start.a, end.a, time));
        }

        /// <summary>
        /// Interpolates a color
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Color Interpolate(Function ease, Color start, Color end, float time)
        {
            var func = EditorFunctions.Funcs[ease];
            return Interpolate(func, start, end, time);
        }

        /// <summary>
        /// Interpolates a color's alpha value
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Color InterpolateAlpha(Function ease, Color start, Color end, float time)
        {
            return new Color(start.r, start.g, start.b, EF.Interpolate(ease, start.a, end.a, time));
        }

        /// <summary>
        /// Interpolates a color's alpha value
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Color InterpolateAlpha(Func<float, float> ease, Color start, Color end, float time)
        {
            return new Color(start.r, start.g, start.b, EF.Interpolate(ease, start.a, end.a, time));
        }

        /// <summary>
        /// Interpolates a color by specified channels
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="channels">Defines the color channels used in interpolation. Axis value of 0 means no interpolation</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Color InterpolateChannels(Function ease, Color start, Color end, bool4 channels, float time)
        {   
            var func = EditorFunctions.Funcs[ease];
            return InterpolateChannels(func, start, end, channels, time);
        }

        /// <summary>
        /// Interpolates a color by specified channels
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="channels">Defines the color channels used in interpolation. Axis value of 0 means no interpolation</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Color InterpolateChannels(Func<float, float> ease, Color start, Color end, bool4 channels, float time)
        {
            var r = channels.x ? EF.Interpolate(ease, start.r, end.r, time) : start.r;
            var g = channels.y ? EF.Interpolate(ease, start.g, end.g, time) : start.g;
            var b = channels.z ? EF.Interpolate(ease, start.b, end.b, time) : start.b;
            var a = channels.w ? EF.Interpolate(ease, start.a, end.a, time) : start.a;
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Interpolates a vector
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Vector3 Interpolate(Func<float, float> ease, Vector3 start, Vector3 end, float time)
        {
            return new Vector3(
                EF.Interpolate(ease, start.x, end.x, time),
                EF.Interpolate(ease, start.y, end.y, time),
                EF.Interpolate(ease, start.z, end.z, time));
        }

        /// <summary>
        /// Interpolates a vector
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Vector3 Interpolate(Function ease, Vector3 start, Vector3 end, float time)
        {
            var func = EditorFunctions.Funcs[ease];
            return Interpolate(func, start, end, time);
        }

        /// <summary>
        /// Interpolates a vector by specified axis
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="axis"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Vector3 InterpolateAxis(Func<float, float> ease, Vector3 start, Vector3 end, bool3 axis, float time)
        {
            var x = axis.x ? EF.Interpolate(ease, start.x, end.x, time) : start.x;
            var y = axis.y ? EF.Interpolate(ease, start.y, end.y, time) : start.y;
            var z = axis.z ? EF.Interpolate(ease, start.z, end.z, time) : start.z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Interpolates a vector by specified axis
        /// </summary>
        /// <param name="ease"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="axis"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Vector3 InterpolateAxis(Function ease, Vector3 start, Vector3 end, bool3 axis, float time)
        {
            var func = EditorFunctions.Funcs[ease];
            return InterpolateAxis(func, start, end, axis, time);
        }
        
    }
}
