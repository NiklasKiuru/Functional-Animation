using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MathUtils
{
    /// <summary>
    /// Returns the derivative of a function at a given point
    /// </summary>
    /// <param name="func"></param>
    /// <param name="value"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static float Derivate(Func<float, float> func, float value, float interval)
    {
        var upper = func(value + interval);
        var lower = func(value - interval);
        return (upper - lower) / (2 * interval);
    }

    /// <summary>
    /// Returns a function that calculates the derivative of the given function
    /// </summary>
    /// <param name="func"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static Func<float, float> DerivateFunc(Func<float, float> func, float interval)
    {
        return (value) => Derivate(func, value, interval);
    }

    /// <summary>
    /// Definite integral of a function between two points
    /// </summary>
    /// <param name="func"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public static float Integrate(Func<float, float> func, float start, float end, float interval)
    {
        var sum = 0f;
        for (var i = start; i < end; i += interval)
        {
            sum += func(i) * interval;
        }
        return sum;
    }

    public static Func<float, float> GenerateWaveSin(float freq, float amplitude)
    {
        return null;
    }

    public static Func<float, float> GenerateWaveCos(float freq, float amplitude)
    {
        var omega = 2 * Mathf.PI * freq;
        return (time) => amplitude * Mathf.Cos(omega * time);
    }

}
