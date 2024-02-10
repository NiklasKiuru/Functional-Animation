using Aikom.FunctionalAnimation;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

public struct VectorInterpolationJob : IJob
{   
    public NativeArray<TransformInterpolationData> Data;
    public NativeArray<RangedFunction> Functions;
    public float DeltaTime;

    public void Execute()
    {
        int startingPoint = 0;

        // Loops through all targets
        for (int i = 0; i < Data.Length; i++)
        {   
            var data = Data[i];
            var validityFlag = false;

            // Loops through the three properties
            for(int j = 0; j < 3; j++)
            {   
                // If the property is not active, skip it
                if (!data.AxisCheck[j][3])
                {   
                    // Graphdata always has 3 pointers per axis minimum
                    startingPoint += 3;
                    continue;
                }

                // Get property time data
                var clock = data.GetClock(j);
                var time = clock.Tick(DeltaTime);
                data.SetClock(j, clock);

                // Validity check
                validityFlag |= (clock.TimeControl == TimeControl.PlayOnce && time >= 1); 
                
                // Loop over all per axis functions
                for(int h = 0; h < 3; h++)
                {
                    var pointerCount = data.GetPointerCount(j);
                    var endingPoint = startingPoint + pointerCount;

                    // Loops through all the function pointers
                    for (int k = startingPoint; k < endingPoint; k++)
                    {
                        var rangedFunc = Functions[k];
                        var startingNode = rangedFunc.Start;
                        var endingNode = rangedFunc.End;
                        if(time >= startingNode.x && time <= endingNode.x)
                        {
                            var mult = rangedFunc.Evaluate(time);
                            data.Current[j][h] = data.From[j][h] + mult * (data.To[j][h] - data.From[j][h]);
                            break;
                        }
                    }
                    startingPoint += pointerCount;
                }
            }

            data.IsActive = validityFlag;
            Data[i] = data;
        }
    }
}

/// <summary>
/// Data for the TransformJobs
/// </summary>
[BurstCompile]
public struct TransformInterpolationData
{
    /// <summary>
    /// From values for each property type and axis
    /// </summary>
    public float3x3 From;

    /// <summary>
    /// Current values for each property type and axis
    /// </summary>
    public float3x3 Current;

    /// <summary>
    /// To values for each property type and axis
    /// </summary>
    public float3x3 To;

    /// <summary>
    /// The clock for the position interpolation
    /// </summary>
    public Clock PositionClock;

    /// <summary>
    /// The number of pointers for the position interpolation
    /// </summary>
    public int PositionPointerCount;

    /// <summary>
    /// The clock for the rotation interpolation
    /// </summary>
    public Clock RotationClock;

    /// <summary>
    /// The number of pointers for the rotation interpolation
    /// </summary>
    public int RotationPointerCount;

    /// <summary>
    /// The clock for the scale interpolation
    /// </summary>
    public Clock ScaleClock;

    /// <summary>
    /// The number of pointers for the scale interpolation
    /// </summary>
    public int ScalePointerCount;

    /// <summary>
    /// Bool matrix to check which axis to calculate or ignore where column for W determines if the property should be included at all
    /// </summary>
    public bool3x4 AxisCheck;

    /// <summary>
    /// Activity check
    /// </summary>
    public bool IsActive;

    public void SetClock(int index, Clock clock)
    {
        switch (index)
        {
            case 0:
                PositionClock = clock;
                break;
            case 1:
                RotationClock = clock;
                break;
            case 2:
                ScaleClock = clock;
                break;
        }
    }

    public Clock GetClock(int index)
    {
        return index switch
        {
            0 => PositionClock,
            1 => RotationClock,
            2 => ScaleClock,
            _ => new Clock()
        };
    }

    public int GetPointerCount(int index)
    {
        return index switch
        {
            0 => PositionPointerCount,
            1 => RotationPointerCount,
            2 => ScalePointerCount,
            _ => 0
        };
    }
}

