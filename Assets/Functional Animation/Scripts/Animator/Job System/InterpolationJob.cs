using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Codice.Client.Commands;

namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct InterpolationJob : IJob
    {   
        public NativeArray<RangedFunction> Functions;
        public NativeArray<FloatInterpolator> Data;
        public NativeArray<float> Results;
        public float DeltaTime;
        public bool HasEvents;
        public NativeList<int> Events;

        public void Execute()
        {   
            var startingPoint = 0;
            for (int i = 0; i < Data.Length; i++)
            {   
                var data = Data[i];
                var time = data.Clock.Tick(DeltaTime);
                var endingPoint = startingPoint + data.Length;
                for (int j = startingPoint; j < endingPoint; j++)
                {
                    var rangedFunc = Functions[j];
                    var startingNode = rangedFunc.Start;
                    var endingNode = rangedFunc.End;
                    if (time >= startingNode.x && time <= endingNode.x)
                    {   
                        Results[i] = rangedFunc.Interpolate(data.From, data.To, time);
                        break;
                    }
                }
                if(data.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                {
                    HasEvents = true;
                    Events.Add(i);
                }
                startingPoint += data.Length;
                Data[i] = data;
            }
        }

        
    }
}

