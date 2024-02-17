using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
namespace Aikom.FunctionalAnimation
{
    [BurstCompile]
    public struct EventData<T> where T : struct
    {
        public int Id;
        public EventFlags Flags;
        public T Value;
    }
}

