using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct MultiTransformInterpolationJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float3> PositionOffsets;
    [ReadOnly] public NativeArray<float3> RotationOffsets;
    [ReadOnly] public NativeArray<float3> ScaleOffsets;
    [ReadOnly] public NativeArray<float3> CurrentValues;
    [ReadOnly] public NativeArray<float3> OriginValues;
    [ReadOnly] public bool3x4 AxisCheck;

    public void Execute(int index, TransformAccess transform)
    {   
        var propArray = new NativeArray<float3>(3, Allocator.Temp);

        for(int i = 0; i < 3; i++)
        {
            propArray[i] = new float3();
            if (AxisCheck[3][i])
            {
                propArray[i] = CurrentValues[i] + GetOffsetValue(i, index);
            }
            else
            {
                var newValue = new float3();
                for(int j = 0; j < 3; j++)
                {
                    if (AxisCheck[j][i])
                        newValue[j] = CurrentValues[i][j];
                    else
                        newValue[j] = GetOriginValue(i)[j];
                }
                propArray[i] = newValue + GetOffsetValue(i, index);
            }
        }

        transform.localPosition = propArray[0];
        var rot = propArray[1];
        transform.localRotation = quaternion.EulerZXY(new float3(math.radians(rot.x), math.radians(rot.y), math.radians(rot.z)));
        transform.localScale = propArray[2];

        propArray.Dispose();        
    }

    float3 GetOffsetValue(int prop, int index)
    {
        return prop switch
        {
            0 => PositionOffsets[index],
            1 => RotationOffsets[index],
            2 => ScaleOffsets[index],
            _ => throw new System.NotImplementedException(),
        };
    }

    float3 GetOriginValue(int prop)
    {
        return prop switch
        {
            0 => OriginValues[0],
            1 => OriginValues[1],
            2 => OriginValues[2],
            _ => throw new System.NotImplementedException(),
        };
    }
}
