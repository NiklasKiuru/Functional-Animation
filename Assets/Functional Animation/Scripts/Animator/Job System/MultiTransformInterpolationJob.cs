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
        var propMatrix = new float3x3();
        for(int i = 0; i < 3; i++)
        {
            propMatrix[i] = new float3();
            if (AxisCheck[3][i])
            {
                propMatrix[i] = CurrentValues[i] + GetOffsetValue(i, index);
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
                propMatrix[i] = newValue + GetOffsetValue(i, index);
            }
        }

        transform.localPosition = propMatrix[0];
        var rot = propMatrix[1];
        transform.localRotation = quaternion.EulerZXY(new float3(math.radians(rot.x), math.radians(rot.y), math.radians(rot.z)));
        transform.localScale = propMatrix[2];      
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
