using System;
using System.Collections.Generic;
using Unity.Burst;

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

        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _linear = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.Linear);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInSine = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInSine);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeOutSine = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseOutSine);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInOutSine = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInOutSine);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInExp = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInExp);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeOutExp = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseOutExp);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInOutExp = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInOutExp);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInBounce = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInBounce);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeOutBounce = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseOutBounce);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInOutBounce = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInOutBounce);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInElastic = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInElastic);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeOutElastic = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseOutElastic);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInOutElastic = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInOutElastic);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInCirc = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInCirc);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeOutCirc = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseOutCirc);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInOutCirc = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInOutCirc);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInBack = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInBack);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeOutBack = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseOutBack);
        private static readonly FunctionPointer<EF.EasingFunctionDelegate> _easeInOutBack = BurstCompiler.CompileFunctionPointer<EF.EasingFunctionDelegate>(EF.EaseInOutBack);

        /// <summary>
        /// Function pointers for jobs
        /// </summary>
        internal static readonly Dictionary<Function, FunctionPointer<EF.EasingFunctionDelegate>> Pointers = new Dictionary<Function, FunctionPointer<EF.EasingFunctionDelegate>>() 
        { 
            {Function.Linear, _linear},
            {Function.EaseInSine, _easeInSine},
            {Function.EaseOutSine, _easeOutSine},
            {Function.EaseInOutSine, _easeInOutSine},
            {Function.EaseInExp, _easeInExp},
            {Function.EaseOutExp, _easeOutExp},
            {Function.EaseInOutExp, _easeInOutExp},
            {Function.EaseInBounce, _easeInBounce},
            {Function.EaseOutBounce, _easeOutBounce},
            {Function.EaseInOutBounce, _easeInOutBounce},
            {Function.EaseInElastic, _easeInElastic},
            {Function.EaseOutElastic, _easeOutElastic},
            {Function.EaseInOutElastic, _easeInOutElastic},
            {Function.EaseInCirc, _easeInCirc},
            {Function.EaseOutCirc, _easeOutCirc},
            {Function.EaseInOutCirc, _easeInOutCirc},
            {Function.EaseInBack, _easeInBack},
            {Function.EaseOutBack, _easeOutBack},
            {Function.EaseInOutBack, _easeInOutBack},
        };
    }
}


