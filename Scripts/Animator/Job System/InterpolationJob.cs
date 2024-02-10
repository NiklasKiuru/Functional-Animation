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
                        Results[i] = rangedFunc.Interpolate(data.Start, data.End, time);
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

        [BurstCompile]
        public unsafe static void ExecuteStatic(in RangedFunction* functions, FloatInterpolator* data, 
            float* results, float delta, out bool hasEvents, int* events, int length)
        {
            hasEvents = false;
            var startingPoint = 0;
            for (int i = 0; i < length; i++)
            {
                var dataPoint = data[i];
                var time = dataPoint.Clock.Tick(delta);
                var endingPoint = startingPoint + dataPoint.Length;
                for (int j = startingPoint; j < endingPoint; j++)
                {
                    var rangedFunc = functions[j];
                    var startingNode = rangedFunc.Start;
                    var endingNode = rangedFunc.End;
                    if (time >= startingNode.x && time <= endingNode.x)
                    {
                        results[i] = rangedFunc.Interpolate(dataPoint.Start, dataPoint.End, time);
                        break;
                    }
                }
                if (dataPoint.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                {
                    hasEvents = true;
                    events[0] = i;
                }
                startingPoint += dataPoint.Length;
                data[i] = dataPoint;
            }
        }
    }

    [BurstCompile]
    public struct FloatInterpolator : IInterpolator<float>
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

        /// <summary>
        /// Internal clock
        /// </summary>
        public Clock Clock;

        public float Current;
        public ExecutionStatus Status { get; set; }

        //ExecutionStatus IInterpolatorHandle<float>.Status { get; set ; }

        public float GetValue()
        {
            return Current;
        }

        public float IncrimentValue()
        {
            throw new NotImplementedException();
        }

        float IInterpolator<float>.GetValue()
        {
            throw new NotImplementedException();
        }

        float IInterpolator<float>.IncrimentValue()
        {
            throw new NotImplementedException();
        }
    }

    public struct VectorInterpolator : IInterpolator<float3>
    {
        public ExecutionStatus Status => throw new System.NotImplementedException();

        ExecutionStatus IInterpolatorHandle<float3>.Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FloatInterpolator x;
        public FloatInterpolator y;
        public FloatInterpolator z;

        public Clock Clock;



        public float3 IncrimentValue()
        {
            throw new NotImplementedException();
        }

        public float3 GetValue()
        {
            throw new NotImplementedException();
        }

        float3 IInterpolator<float3>.IncrimentValue()
        {
            throw new NotImplementedException();
        }

        float3 IInterpolator<float3>.GetValue()
        {
            throw new NotImplementedException();
        }
    }
}

