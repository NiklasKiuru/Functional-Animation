using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aikom.FunctionalAnimation;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

public class CallbackHandleTest : MonoBehaviour
{
    private CallbackRegistry _registry;
    private NativeList<int> _events;
    private NativeList<RangedFunction> _functions;
    private NativeList<FloatInterpolator> _floats;
    private NativeList<float> _results;
    private InterpolationJob _job;

    private void Start()
    {
        _results = new NativeList<float>(Allocator.Persistent);
        _functions = new NativeList<RangedFunction>(Allocator.Persistent);
        _floats = new NativeList<FloatInterpolator>(Allocator.Persistent);
        _events = new NativeList<int>(Allocator.Persistent);
        for(int i = 0; i < 100; i++)
        {
            var data = new FloatInterpolator
            {
                Length = 1,
                Start = 0,
                End = 1,
                Clock = new Clock(0.2f, TimeControl.PlayOnce)
            };

            data.OnComplete(() => _results[0] = 0);
            _floats.Add(data);
            _functions.Add(new RangedFunction
            {
                Start = new float2(0, 0),
                End = new float2(1, 1),
                Pointer = EditorFunctions.Pointers[Function.Linear]
            });
            _results.Add(0);
            _events.Add(0);
        }
        

        _registry = new CallbackRegistry(_events);
    }

    private unsafe void Update()
    {
        //_job = new InterpolationJob
        //{
        //    Data = _floats,
        //    Functions = _functions,
        //    Results = _results,
        //    DeltaTime = Time.deltaTime,
        //    Events = _events,
        //};
        //_job.Run();
        InterpolationJob.ExecuteStatic((RangedFunction*)_functions.GetUnsafeReadOnlyPtr(), 
            (FloatInterpolator*)_floats.GetUnsafePtr(), (float*)_results.GetUnsafePtr(), Time.deltaTime, out var hasEvents, (int*)_events.GetUnsafePtr(), _floats.Length);
        if (hasEvents)
        {
            CallbackRegistry.SetDirty();
            
        }
            
    }

    private void OnDestroy()
    {
        if(_results.IsCreated) _results.Dispose();
        if(_functions.IsCreated) _functions.Dispose();
        if(_floats.IsCreated) _floats.Dispose();
        if (_events.IsCreated) _events.Dispose();
    }

}
