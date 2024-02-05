using Aikom.FunctionalAnimation;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;

public struct TransformInterpolationJob : IJobParallelForTransform
{   
    public NativeArray<TransformInterpolationData> Data;
    public NativeArray<FunctionPointer<EF.EasingFunctionDelegate>> Pointers;
    public NativeArray<float2> TimelineData;
    public float DeltaTime;

    public void Execute(int index, TransformAccess transform)
    {
         
    }
}

public struct TransformInterpolationData
{
    public float3x3 From;
    public float3x3 To;
    public Clock PositionClock;
    public Clock RotationClock;
    public Clock ScaleClock;
    public bool3x4 AxisCheck;
    public bool IsActive;
}
