using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System;

namespace Aikom.FunctionalAnimation.Examples
{
    public class InterpolationTest : MonoBehaviour
    {   
        public enum TestType
        {
            Job,
            OOP
        }

        [SerializeField] private TestType _type;
        [SerializeField] private int _amount;

        private InterpolatorGroup _data;
        private NativeArray<float> _results;
        private InterpolationJob _job;
        private Interpolator<float>[] _interpolators;

        private void Start()
        {   
            _interpolators = new Interpolator<float>[_amount];
            var graphs = new GraphData[_amount];
            var endingPoints = new float2[_amount];
            var speeds = new float[_amount];
            var timeControls = new TimeControl[_amount];
            for(int i = 0; i < _amount; i++)
            {
                endingPoints[i] = new float2(0, 1);
                var graph = new GraphData();
                graph.AddFunction(Function.EaseOutBounce, new Vector2(0.5f, 0));
                graph.AddFunction(Function.EaseInBounce, new Vector2(0.5f, 1));
                graphs[i] = graph;
                speeds[i] = 1;
                timeControls[i] = TimeControl.Loop;

                _interpolators[i] = new Interpolator<float>(GetIncriment(graph), null, 1, 0, 1, TimeControl.Loop);
            }


            _data = new InterpolatorGroup(graphs, endingPoints, speeds, timeControls);
            _results = new NativeArray<float>(_amount, Allocator.Persistent);

            Func<float, float, float, float> GetIncriment(GraphData data)
            {
                var func = data.GenerateFunction();
                return (t, start, end) => { return EF.Interpolate(func, start, end, t); };
            }
        }

        private void Update()
        {   
            if(_type == TestType.OOP)
            {
                for(int i = 0; i < _interpolators.Length; i++)
                {
                    _interpolators[i].Run();
                }
            }
            else
            {
                _job = new InterpolationJob
                {
                    FunctionPointers = _data.FuncPointers,
                    TimelineData = _data.TimelineData,
                    TimeData = _data.LerpData,
                    Clocks = _data.Clocks,
                    DeltaTime = Time.deltaTime,
                    Results = _results
                };
                _job.Run();
                //_job.Execute();
            }

            //_clock.Tick();
            //var time = _clock.Time;
            //for (int i = 0; i < _data.TimeData.Length; i++)
            //{   
            //    var timeData = _data.TimeData[i];
            //    timeData.Value = time;
            //    _data.TimeData[i] = timeData;
                
            //}

            //_job = new InterpolationJob
            //{
            //    FunctionPointers = _data.FuncPointers,
            //    TimelineData = _data.TimelineData,
            //    TimeData = _data.TimeData,
            //    Results = _results
            //};
            //_job.Run();
            ////_job.Execute();
        }

        private void OnDisable()
        {
            _data.Dispose();
            _results.Dispose();
        }
    }
}

