using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using System;

namespace Aikom.FunctionalAnimation
{
    public class InterpolatorGroup : IDisposable
    {
        public NativeList<FunctionPointer<EF.EasingFunctionDelegate>> FuncPointers;
        public NativeList<float2> TimelineData;
        public NativeList<InterpolationData> LerpData;
        public NativeList<Clock> Clocks;

        private InterpolationJob _job;
        private NativeList<float> _results;

        public InterpolatorGroup(GraphData[] data, float2[] endPoints, float[] speed, TimeControl[] ctrl)
        {
            LerpData = new NativeList<InterpolationData>(data.Length, Allocator.Persistent);
            Clocks = new NativeList<Clock>(data.Length, Allocator.Persistent);

            var pointers = 0;
            var timeline = 0;
            for(int i = 0; i < data.Length; i++)
            {
                LerpData.Add(new InterpolationData
                {
                    Length = data[i].Functions.Count,
                    Start = endPoints[i].x,
                    End = endPoints[i].y
                }); 
                pointers += data[i].Functions.Count;
                timeline += data[i].Nodes.Count;

                Clocks.Add(new Clock(speed[i], ctrl[i]));
            }

            FuncPointers = new NativeList<FunctionPointer<EF.EasingFunctionDelegate>>(pointers, Allocator.Persistent);
            TimelineData = new NativeList<float2>(timeline, Allocator.Persistent);
            _results = new NativeList<float>(data.Length, Allocator.Persistent);

            var timeIndex = 0;
            var funcIndex = 0;
            for (int i = 0; i < data.Length; i++)
            {
                
                foreach (var node in data[i].Nodes)
                {
                    TimelineData[timeIndex] = node;
                    timeIndex++;
                }

                var funcs = data[i].GetPointerArray();
                for(int j = 0; j < funcs.Length; j++)
                {
                    FuncPointers[funcIndex] = funcs[j];
                    funcIndex++;
                }
            }
        }

        public InterpolatorGroup()
        {
            FuncPointers = new NativeList<FunctionPointer<EF.EasingFunctionDelegate>>(0, Allocator.Persistent);
            TimelineData = new NativeList<float2>(0, Allocator.Persistent);
            LerpData = new NativeList<InterpolationData>(0, Allocator.Persistent);
            Clocks = new NativeList<Clock>(0, Allocator.Persistent);
            _results = new NativeList<float>(0, Allocator.Persistent);
        }

        /// <summary>
        /// Adds a new interpolation target
        /// </summary>
        /// <param name="data"></param>
        /// <param name="range"></param>
        /// <param name="speed"></param>
        /// <param name="ctrl"></param>
        public void Add(GraphData data, float2 range, float speed, TimeControl ctrl)
        {
            var funcs = data.GetPointerArray();
            for(int i = 0; i < funcs.Length; i++)
            {
                FuncPointers.Add(funcs[i]);
            }
            foreach (var node in data.Nodes)
            {
                TimelineData.Add(node);
            }
            LerpData.Add(new InterpolationData
            {
                Length = data.Functions.Count,
                Start = range.x,
                End = range.y
            });
            Clocks.Add(new Clock(speed, ctrl));
            _results.Add(0);
        }

        /// <summary>
        /// Removes the interpolation target at the given index
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            var length = LerpData[index].Length;
            var start = 0;
            var cycles = 0;

            // Now this is actually very inefficient, get starting position of pointer array
            for(int i = 0; i < LerpData.Length; i++)
            {
                if (i == index)
                    break;
                start += LerpData[i].Length;
                cycles++;
            }

            // Remove the function pointers and timeline data. Cycles tells the amount of interpolation targets have been passed in previous iteration
            for(int i = 0; i < length; i++)
            {
                FuncPointers.RemoveAtSwapBack(start);
                TimelineData.RemoveAtSwapBack(start + cycles);
                start++;
            }

            Clocks.RemoveAtSwapBack(index);
            LerpData.RemoveAtSwapBack(index);
            _results.RemoveAtSwapBack(index);
        }

        /// <summary>
        /// Runs the interpolation job
        /// </summary>
        public void Run()
        {
            _job = new InterpolationJob
            {
                FunctionPointers = FuncPointers,
                TimelineData = TimelineData,
                TimeData = LerpData,
                Clocks = Clocks,
                Results = _results,
                DeltaTime = Time.deltaTime
            };
            _job.Run();
        }

        /// <summary>
        /// Disposes unmanaged memory
        /// </summary>
        public void Dispose()
        {
            FuncPointers.Dispose();
            TimelineData.Dispose();
            LerpData.Dispose();
            Clocks.Dispose();
            _results.Dispose();
        }
    }
}

