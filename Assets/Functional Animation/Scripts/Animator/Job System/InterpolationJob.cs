using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct InterpolationJob : IJob
    {
        [ReadOnly] public NativeArray<FunctionPointer<EF.EasingFunctionDelegate>> FunctionPointers;
        [ReadOnly] public NativeArray<float2> TimelineData;
        [ReadOnly] public NativeArray<InterpolationData> TimeData;
        public NativeArray<Clock> Clocks;
        public NativeArray<float> Results;
        public float DeltaTime;

        public void Execute()
        {   
            int startingPoint = 0;
            for (int i = 0; i < TimeData.Length; i++)
            {   
                var clock = Clocks[i];
                var timeData = TimeData[i];
                var time = clock.Tick(DeltaTime);
                Clocks[i] = clock;
                var endingPoint = startingPoint + timeData.Length;
                for (int j = startingPoint; j <  endingPoint; j++)
                {
                    var timelineIndex = j + i;
                    if (time <= TimelineData[timelineIndex].x && time < TimelineData[timelineIndex + 1].x)
                    {
                        var startingNode = TimelineData[timelineIndex];
                        var endingNode = TimelineData[timelineIndex + 1];
                        var amplitude = endingNode.y - startingNode.y;
                        var totalTime = 1 - (1 - endingNode.x) - startingNode.x;
                        var t = (time - startingNode.x) * (1 / totalTime);
                        var mult = FunctionPointers[j].Invoke(t) * amplitude + startingNode.y;
                        Results[i] = timeData.Start + mult * (timeData.End - timeData.Start);
                        break;
                    }
                    else if (time == TimelineData[timelineIndex + 1].x)
                    {
                        Results[i] = timeData.Start + (timeData.End - timeData.Start) * TimelineData[endingPoint].y;
                    }
                }
                startingPoint += timeData.Length;
            }
        }
    }

    [BurstCompatible]
    public struct InterpolationData
    {   
        /// <summary>
        /// Length of the function pointer array
        /// </summary>
        public int Length;

        /// <summary>
        /// Interpolation start value
        /// </summary>
        public float Start;

        /// <summary>
        /// Interpolation end value
        /// </summary>
        public float End;
    }
}

