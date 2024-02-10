using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using System;

namespace Aikom.FunctionalAnimation
{
    public class InterpolatorGroup : IDisposable
    {
        private NativeList<FloatInterpolator> _lerpData;
        private NativeList<RangedFunction> _functions;
        private InterpolationJob _job;
        private NativeList<float> _results;

        public NativeList<float> Results => _results;

        public InterpolatorGroup(GraphData[] data, float2[] endPoints, float[] speed, TimeControl[] ctrl)
        {
            _lerpData = new NativeList<FloatInterpolator>(data.Length, Allocator.Persistent);
            _functions = new NativeList<RangedFunction>(0, Allocator.Persistent);
            _results = new NativeList<float>(data.Length, Allocator.Persistent);

            for(int i = 0; i < data.Length; i++)
            {   
                var dataPoint = new FloatInterpolator
                {
                    Length = data[i].Functions.Count,
                    Start = endPoints[i].x,
                    End = endPoints[i].y,
                    Clock = new Clock(speed[i], ctrl[i]),
                    
                };

                _lerpData.Add(dataPoint);
                var funcs = data[i].GetRangedFunctionArray();
                for(int j = 0; j < funcs.Length; j++)
                {
                    _functions.Add(funcs[j]);
                }

                _results.Add(0);
            }
            
        }

        public InterpolatorGroup()
        {
            _lerpData = new NativeList<FloatInterpolator>(0, Allocator.Persistent);
            _results = new NativeList<float>(0, Allocator.Persistent);
            _functions = new NativeList<RangedFunction>(0, Allocator.Persistent);
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
            //var funcs = data.GetRangedFunctionArray();
            //for(int i = 0; i < funcs.Length; i++)
            //{
            //    FuncPointers.Add(funcs[i]);
            //}
            //LerpData.Add(new InterpolationData
            //{
            //    Length = data.Functions.Count,
            //    Start = range.x,
            //    End = range.y
            //});
            //_results.Add(0);
        }

        /// <summary>
        /// Removes the interpolation target at the given index
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            var length = _lerpData[index].Length;
            var start = 0;

            // Now this is actually very inefficient, get starting position of pointer array
            for(int i = 0; i < _lerpData.Length; i++)
            {
                if (i == index)
                    break;
                start += _lerpData[i].Length;
            }
            _lerpData.RemoveAtSwapBack(index);
            _results.RemoveAtSwapBack(index);
        }

        /// <summary>
        /// Runs the interpolation job
        /// </summary>
        public void Run()
        {
            _job = new InterpolationJob
            {
                Data = _lerpData,
                Results = _results,
                DeltaTime = Time.deltaTime,
                Functions = _functions
            };
            _job.Run();
            
        }

        private void Test(float val)
        {
            Debug.Log(val);
        }

        /// <summary>
        /// Disposes unmanaged memory
        /// </summary>
        public void Dispose()
        {   
            _functions.Dispose();
            _lerpData.Dispose();
            _results.Dispose();
        }
    }
}

