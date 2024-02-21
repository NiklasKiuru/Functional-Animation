using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

[BurstCompile]
public struct SyncTransformGroupJob : IJobParallelForTransform
{
    [ReadOnly] public NativeArray<float3x3> Offsets;
    [ReadOnly] public float3x3 CurrentValues;
    [ReadOnly] public float3x3 OriginValues;
    [ReadOnly] public bool3x4 AxisCheck;
    [ReadOnly] public bool3 PropCheck;

    public void Execute(int index, TransformAccess transform)
    {
        var propMatrix = new float3x3();
        for(int i = 0; i < 3; i++)
        {
            propMatrix[i] = new float3();
            if (AxisCheck[3][i])
            {
                propMatrix[i] = CurrentValues[i] + Offsets[index][i];
            }
            else
            {
                var newValue = new float3();
                for(int j = 0; j < 3; j++)
                {
                    if (AxisCheck[j][i])
                        newValue[j] = CurrentValues[i][j];
                    else
                        newValue[j] = OriginValues[i][j];
                }
                propMatrix[i] = newValue + Offsets[index][i];
            }
        }

        if(PropCheck.x)
            transform.localPosition = propMatrix[0];
        if (PropCheck.y)
        {
            var rot = propMatrix[1];
            transform.localRotation = quaternion.EulerZXY
                    (new float3(math.radians(rot.x), math.radians(rot.y), math.radians(rot.z)));
        }
        if(PropCheck.z)
            transform.localScale = propMatrix[2];      
    }
}
