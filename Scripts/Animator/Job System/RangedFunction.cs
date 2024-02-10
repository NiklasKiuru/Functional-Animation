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

        public RangedFunction(FunctionPointer<EF.EasingFunctionDelegate> pointer, float2 start, float2 end)
        {
            Pointer = pointer;
            Start = start;
            End = end;
        }

        public RangedFunction(Function function = Function.Linear)
        {
            Pointer = EditorFunctions.Pointers[function];
            Start = float2.zero;
            End = new float2(1,1);
        }

        public RangedFunction(Function function, float startVal, float endVal)
        {
            Pointer = EditorFunctions.Pointers[function];
            Start = new float2(0, startVal);
            End = new float2(1, endVal);
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

