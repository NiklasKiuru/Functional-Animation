using System;
using System.Collections.Generic;

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
    }
}


