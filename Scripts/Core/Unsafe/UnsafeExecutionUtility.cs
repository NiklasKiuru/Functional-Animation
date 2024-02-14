using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Extremely fast interpolation functions for 1, 2, 3 and 4 dimensional vectors
    /// </summary>
    /// <remarks>There is a lot of repetition here which is quite difficult to get rid of due to burst and C# limitations</remarks>
    [BurstCompile]
    public static class UnsafeExecutionUtility
    {
        [BurstCompile]
        public unsafe static void InterpolateFloats(in RangedFunction* functions, FloatInterpolator* data, 
            float delta, out bool hasEvents, FlagIndexer<float>* events, int length)
        {
            hasEvents = false;
            var startingPoint = 0;
            for (int i = 0; i < length; i++)
            {
                var dataPoint = data[i];
                if (dataPoint.Status == ExecutionStatus.Paused)
                {   
                    startingPoint += dataPoint.Length;
                    continue;
                }
                
                if(dataPoint.Clock.Time == 0 && (dataPoint.PassiveFlags & EventFlags.OnStart) == EventFlags.OnStart)
                    dataPoint.ActiveFlags |= EventFlags.OnStart;

                var time = dataPoint.Clock.Tick(delta);
                var endingPoint = startingPoint + dataPoint.Length;
                for (int j = startingPoint; j < endingPoint; j++)
                {
                    var rangedFunc = functions[j];
                    var startingNode = rangedFunc.Start;
                    var endingNode = rangedFunc.End;
                    if (time >= startingNode.x && time <= endingNode.x)
                    {
                        dataPoint.Current = rangedFunc.Interpolate(dataPoint.From, dataPoint.To, time);
                        if((dataPoint.PassiveFlags & EventFlags.OnUpdate) == EventFlags.OnUpdate)
                            dataPoint.ActiveFlags |= EventFlags.OnUpdate;
                        break;
                    }
                }
                if (dataPoint.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                {   
                    if((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete)
                        dataPoint.ActiveFlags |= EventFlags.OnComplete;
                    dataPoint.Status = ExecutionStatus.Completed;
                }
                if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete &&
                    dataPoint.Status == ExecutionStatus.Completed)
                    dataPoint.ActiveFlags |= EventFlags.OnComplete;

                startingPoint += dataPoint.Length;
                var hasActiveFlags = dataPoint.ActiveFlags != EventFlags.None;
                hasEvents = hasActiveFlags || dataPoint.Status == ExecutionStatus.Completed;
                var flagIndexer = events[i];
                if (hasActiveFlags)
                {
                    flagIndexer.Id = dataPoint.InternalId;
                    flagIndexer.Flags = dataPoint.ActiveFlags;
                    flagIndexer.Value = dataPoint.Current;
                }
                else
                {
                    flagIndexer.Id = -1;
                }
                dataPoint.ActiveFlags = EventFlags.None;
                data[i] = dataPoint;
                events[i] = flagIndexer;
            }
        }

        [BurstCompile]
        public unsafe static void Interpolate4Floats(in RangedFunction* functions, Vector4Interpolator* data,
            float delta, out bool hasEvents, FlagIndexer<float4>* events, int length)
        {
            hasEvents = false;
            var startingPoint = 0;
            for (int i = 0; i < length; i++)
            {
                var dataPoint = data[i];
                if (dataPoint.Status == ExecutionStatus.Paused)
                {   
                    startingPoint += dataPoint.Length;
                    continue;
                }
                    
                if (dataPoint.Clock.Time == 0 && (dataPoint.PassiveFlags & EventFlags.OnStart) == EventFlags.OnStart)
                    dataPoint.ActiveFlags |= EventFlags.OnStart;
                var time = dataPoint.Clock.Tick(delta);
                var newVal = dataPoint.Current;
                for (int axis = 0; axis < 4; axis++)
                {
                    if (!dataPoint.AxisCheck[axis])
                        continue;
                    var endingPoint = startingPoint + dataPoint.Stride[axis];
                    for(int j = startingPoint; j < endingPoint; j++)
                    {
                        var rangedFunc = functions[j];
                        var startingNode = rangedFunc.Start;
                        var endingNode = rangedFunc.End;
                        if (time >= startingNode.x && time <= endingNode.x)
                        {
                            newVal[axis] = rangedFunc.Interpolate(dataPoint.From[axis], dataPoint.To[axis], time);
                            if ((dataPoint.PassiveFlags & EventFlags.OnUpdate) == EventFlags.OnUpdate)
                                dataPoint.ActiveFlags |= EventFlags.OnUpdate;
                            break;
                        }
                    }
                    startingPoint = endingPoint;
                }
                dataPoint.Current = newVal;
                if (dataPoint.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                {
                    if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete)
                        dataPoint.ActiveFlags |= EventFlags.OnComplete;
                    dataPoint.Status = ExecutionStatus.Completed;
                }
                if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete &&
                    dataPoint.Status == ExecutionStatus.Completed)
                    dataPoint.ActiveFlags |= EventFlags.OnComplete;

                var hasActiveFlags = dataPoint.ActiveFlags != EventFlags.None;
                var flagIndexer = events[i];
                if (hasActiveFlags)
                {
                    hasEvents = true;
                    flagIndexer.Id = dataPoint.InternalId;
                    flagIndexer.Flags = dataPoint.ActiveFlags;
                    flagIndexer.Value = dataPoint.Current;
                }
                else
                {
                    flagIndexer.Id = -1;
                }
                dataPoint.ActiveFlags = EventFlags.None;
                data[i] = dataPoint;
                events[i] = flagIndexer;
            }
        }

        [BurstCompile]
        public unsafe static void Interpolate3Floats(in RangedFunction* functions, Vector3Interpolator* data,
            float delta, out bool hasEvents, FlagIndexer<float3>* events, int length)
        {
            hasEvents = false;
            var startingPoint = 0;
            for (int i = 0; i < length; i++)
            {
                var dataPoint = data[i];
                if (dataPoint.Status == ExecutionStatus.Paused)
                {
                    startingPoint += dataPoint.Length;
                    continue;
                }

                if (dataPoint.Clock.Time == 0 && (dataPoint.PassiveFlags & EventFlags.OnStart) == EventFlags.OnStart)
                    dataPoint.ActiveFlags |= EventFlags.OnStart;
                var time = dataPoint.Clock.Tick(delta);
                var newVal = dataPoint.Current;
                for (int axis = 0; axis < 3; axis++)
                {
                    if (!dataPoint.AxisCheck[axis])
                        continue;
                    var endingPoint = startingPoint + dataPoint.Stride[axis];
                    for (int j = startingPoint; j < endingPoint; j++)
                    {
                        var rangedFunc = functions[j];
                        var startingNode = rangedFunc.Start;
                        var endingNode = rangedFunc.End;
                        if (time >= startingNode.x && time <= endingNode.x)
                        {
                            newVal[axis] = rangedFunc.Interpolate(dataPoint.From[axis], dataPoint.To[axis], time);
                            if ((dataPoint.PassiveFlags & EventFlags.OnUpdate) == EventFlags.OnUpdate)
                                dataPoint.ActiveFlags |= EventFlags.OnUpdate;
                            break;
                        }
                    }
                    startingPoint = endingPoint;
                }
                dataPoint.Current = newVal;
                if (dataPoint.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                {
                    if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete)
                        dataPoint.ActiveFlags |= EventFlags.OnComplete;
                    dataPoint.Status = ExecutionStatus.Completed;
                }
                var hasActiveFlags = dataPoint.ActiveFlags != EventFlags.None;
                var flagIndexer = events[i];
                if (hasActiveFlags)
                {
                    hasEvents = true;
                    flagIndexer.Id = dataPoint.InternalId;
                    flagIndexer.Flags = dataPoint.ActiveFlags;
                    flagIndexer.Value = dataPoint.Current;
                }
                else
                {
                    flagIndexer.Id = -1;
                }
                dataPoint.ActiveFlags = EventFlags.None;
                data[i] = dataPoint;
                events[i] = flagIndexer;
            }
        }

        [BurstCompile]
        public unsafe static void Interpolate2Floats(in RangedFunction* functions, Vector2Interpolator* data,
            float delta, out bool hasEvents, FlagIndexer<float2>* events, int length)
        {
            hasEvents = false;
            var startingPoint = 0;
            for (int i = 0; i < length; i++)
            {
                var dataPoint = data[i];
                if (dataPoint.Status == ExecutionStatus.Paused)
                {
                    startingPoint += dataPoint.Length;
                    continue;
                }

                if (dataPoint.Clock.Time == 0 && (dataPoint.PassiveFlags & EventFlags.OnStart) == EventFlags.OnStart)
                    dataPoint.ActiveFlags |= EventFlags.OnStart;
                var time = dataPoint.Clock.Tick(delta);
                var newVal = dataPoint.Current;
                for (int axis = 0; axis < 2; axis++)
                {
                    if (!dataPoint.AxisCheck[axis])
                        continue;
                    var endingPoint = startingPoint + dataPoint.Stride[axis];
                    for (int j = startingPoint; j < endingPoint; j++)
                    {
                        var rangedFunc = functions[j];
                        var startingNode = rangedFunc.Start;
                        var endingNode = rangedFunc.End;
                        if (time >= startingNode.x && time <= endingNode.x)
                        {
                            newVal[axis] = rangedFunc.Interpolate(dataPoint.From[axis], dataPoint.To[axis], time);
                            if ((dataPoint.PassiveFlags & EventFlags.OnUpdate) == EventFlags.OnUpdate)
                                dataPoint.ActiveFlags |= EventFlags.OnUpdate;
                            break;
                        }
                    }
                    startingPoint = endingPoint;
                }
                dataPoint.Current = newVal;
                if (dataPoint.Clock.TimeControl == TimeControl.PlayOnce && time >= 1)
                {
                    if ((dataPoint.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete)
                        dataPoint.ActiveFlags |= EventFlags.OnComplete;
                    dataPoint.Status = ExecutionStatus.Completed;
                }
                var hasActiveFlags = dataPoint.ActiveFlags != EventFlags.None;
                var flagIndexer = events[i];
                if (hasActiveFlags)
                {
                    hasEvents = true;
                    flagIndexer.Id = dataPoint.InternalId;
                    flagIndexer.Flags = dataPoint.ActiveFlags;
                    flagIndexer.Value = dataPoint.Current;
                }
                else
                {
                    flagIndexer.Id = -1;
                }
                dataPoint.ActiveFlags = EventFlags.None;
                data[i] = dataPoint;
                events[i] = flagIndexer;
            }
        }
    }
}

