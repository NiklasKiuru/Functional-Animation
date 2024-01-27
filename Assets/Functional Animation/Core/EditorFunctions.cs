using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Functions used in editor dropdown lists
    /// </summary>
    public enum Function
    {
        Linear,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExp,
        EaseOutExp,
        EaseInOutExp,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
    }

    /// <summary>
    /// Class that holds a dictionary for easing functions that can be found by using the Function enum
    /// With this it is easy to assign different easing functionality by just using the Function enum in editor fields
    /// </summary>
    public class EditorFunctions
    {
        public static readonly Dictionary<Function, Func<float, float>> Funcs = new Dictionary<Function, Func<float, float>>()
    {
        { Function.Linear, f => { return f; } },
        { Function.EaseInSine, EF.EaseInSine },
        { Function.EaseOutSine, EF.EaseOutSine },
        { Function.EaseInOutSine, EF.EaseInOutSine },
        { Function.EaseInExp, EF.EaseInExp },
        { Function.EaseOutExp, EF.EaseOutExp },
        { Function.EaseInOutExp, EF.EaseInOutExp },
        { Function.EaseInBounce, EF.EaseInBounce },
        { Function.EaseOutBounce, EF.EaseOutBounce },
        { Function.EaseInOutBounce, EF.EaseInOutBounce },
        { Function.EaseInElastic, EF.EaseInElastic },
        { Function.EaseOutElastic, EF.EaseOutElastic },
        { Function.EaseInOutElastic, EF.EaseInOutElastic },
        { Function.EaseInCirc, EF.EaseInCirc },
        { Function.EaseOutCirc, EF.EaseOutCirc },
        { Function.EaseInOutCirc, EF.EaseInOutCirc },
        { Function.EaseInBack, EF.EaseInBack },
        { Function.EaseOutBack, EF.EaseOutBack },
        { Function.EaseInOutBack, EF.EaseInOutBack },
    };



        public void Test()
        {
            float time = 0;

            // Basic case of vector interpolation
            var vec1 = new Vector3();
            var vec2 = new Vector3();
            var vec3 = EF.Interpolate(Function.EaseInSine, vec1, vec2, v => { return new float[] { v.x, v.y, v.z }; }, a => { return new Vector3(a[0], a[1], a[2]); }, time);

            // Combining two easing functions
            // Combining easing functions will create a new function that executes a new 
            var customEase = EF.Combine(Function.EaseInSine, Function.EaseOutSine);
            var vec4 = EF.Interpolate(customEase, vec1, vec2, VecToFloats, FloatsToVec, time * 0.5f);

            // Combining two easing functions and inverting the second one for fluent continuous animation
            var customEase2 = EF.Combine(Funcs[Function.EaseOutSine], EF.Invert(Function.EaseInOutSine));
            var vec5 = EF.Interpolate(customEase2, vec1, vec2, VecToFloats, FloatsToVec, time * 0.5f);

            // Example for color interpolation
            // Combines the previous two custom easing functions for a total of 4
            var col1 = new Color();
            var col2 = new Color();
            var col3 = EF.Interpolate(EF.Combine(customEase, customEase2), col1, col2, ColorToFloats, FloatsToColor, time * 0.25f);

            // Due to custom conversion functions it is possible to define only partial interpolation
            // In this example the interpolation is only done on the target vectors x and y components
            var vec6 = EF.Interpolate(Function.EaseInOutExp, vec1, vec2, v => { return new float[] { v.x, v.y }; }, a => { return new Vector3(a[0], a[1], vec1.z); }, time);

            // It is also possible to define a custom data struct and only use some values of the target
            var mystruct1 = new MyStruct();
            var mystruct2 = new MyStruct();
            var mystruct3 = EF.Interpolate(Function.EaseOutExp, mystruct1, mystruct2, m => { return new float[] { m.Value1, m.Value2 }; }, f => { return new MyStruct(f[0], f[1], mystruct1.Value3Int); }, time);

            // Assigning a struct as IInterpolateable<T> the default conversion can be written on the struct itself
            // Here instead of creating a new function on every call the integer value on MyStruct is automatically assigned from the original data
            var mystruct4 = EF.Interpolate(Function.EaseInSine, mystruct1, mystruct2, time);

            // With this it is possible albeit niche to interpolate between two different structs if they both implement the same type of IInterpolateable
            // I have no idea if this is needed in any case and using incorrect construction functions can lead into errors
            // In this example it is evident how MyStruct2 is also being restricted to be interpolateable with only MyStruct which might not be desireable
            var ms2 = new MyStruct2();
            var mystruct5 = EF.Interpolate(Function.EaseInSine, mystruct1, ms2, time);

            // In order to make some native structs easily interpolateable the UnityExtensions namespace contains
            // extension methods for most of the basic structs do to this natively 

            // Conversion functions are practical to store somewhere if used multiple times
            static float[] VecToFloats(Vector3 vec) => new float[] { vec.x, vec.y, vec.z };
            static Vector3 FloatsToVec(float[] floats) => new Vector3(floats[0], floats[1], floats[2]);
            static float[] ColorToFloats(Color col) => new float[] { col.r, col.g, col.b, col.a };
            static Color FloatsToColor(float[] floats) => new Color(floats[0], floats[1], floats[2], floats[3]);
        }

        private struct MyStruct : IInterpolateable<MyStruct>
        {
            public float Value1;
            public float Value2;
            public int Value3Int;

            public MyStruct(float val1, float val2, int value3)
            {
                Value1 = val1; Value2 = val2; Value3Int = value3;
            }

            public MyStruct ReConstruct<T>(float[] values, MyStruct original) where T : IInterpolateable<MyStruct>
            {
                return new MyStruct(values[0], values[1], original.Value3Int);
            }

            public float[] Deconstruct()
            {
                return new float[] { Value1, Value2, };
            }
        }

        private struct MyStruct2 : IInterpolateable<MyStruct>
        {
            public float[] Deconstruct()
            {
                throw new NotImplementedException();
            }

            public MyStruct ReConstruct<D>(float[] values, MyStruct origin) where D : IInterpolateable<MyStruct>
            {
                throw new NotImplementedException();
            }
        }
    }
}


