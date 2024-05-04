using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct RangedFunction
    {   
        /// <summary>
        /// EF Delegate pointer
        /// </summary>
        public FunctionPointer<EF.EasingFunctionDelegate> Pointer;

        /// <summary>
        /// Starting time as Start.x and starting value as Start.y
        /// </summary>
        public float2 Start;

        /// <summary>
        /// Ending time as End.x and ending value as End.y
        /// </summary>
        public float2 End;

        /// <summary>
        /// Creates a ranged function with defined starting and ending points
        /// </summary>
        /// <param name="function"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RangedFunction(Function function, float2 start, float2 end)
        {
            Pointer = BurstFunctionCache.GetCachedPointer(function);
            Start = start;
            End = end;
        }

        /// <summary>
        /// Creates a ranged function from a known alias
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RangedFunction(FunctionAlias alias, float2 start, float2 end)
        {
            Start = start;
            End = end;
            Pointer = BurstFunctionCache.GetCachedPointer(alias);
        }

        /// <summary>
        /// Creates a ranged function from a delegate
        /// </summary>
        /// <param name="del">The used delegate must fulfill BurstCompiler function pointer compilation requirements and must have <see cref="EFunctionAttribute"/></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RangedFunction(EF.EasingFunctionDelegate del, float2 start, float2 end)
        {
            Start = start;
            End = end;
            Pointer = BurstFunctionCache.GetCachedPointer(del);
        }

        /// <summary>
        /// Creates a ranged function with predefined range of [0,0] -> [1,1]
        /// </summary>
        /// <param name="function"></param>
        public RangedFunction(Function function)
        {
            Pointer = BurstFunctionCache.GetCachedPointer(function);
            Start = float2.zero;
            End = new float2(1,1);
        }

        /// <summary>
        /// Evaluates the function at a given time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Evaluate(float time)
        {
            var amplitude = End.y - Start.y;
            var totalTime = 1 - (1 - End.x) - Start.x;
            var t = (time - Start.x) * (1 / totalTime);
            return Pointer.Invoke(t) * amplitude + Start.y;
        }

        /// <summary>
        /// Interpolates between two values using the function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Interpolate(float from, float to, float time)
        {
            var mult = Evaluate(time);
            return from + mult * (to - from);
        }
    }
}

