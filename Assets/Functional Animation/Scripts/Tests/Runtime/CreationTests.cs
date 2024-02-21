using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using Aikom.FunctionalAnimation.Utility;

namespace Aikom.FunctionalAnimation.Tests
{
    public class CreationTests
    {
        [Test]
        public void CreateFloats_Test()
        {
            var from = 2;
            var to = 1;
            var duration = 2;
            var list = new List<ProcessId> 
            {
                EF.Create(from, to, duration, Function.EaseOutElastic).GetIdentifier(),
                EF.Create(from, to, duration, new RangedFunction(Function.EaseOutCirc)).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData()).GetIdentifier(),
                EF.CreateNonAlloc(from, to, duration, Function.EaseOutBack, TimeControl.Loop, 1),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.PlayOnce, 1, (v) => Debug.Log(v)),
            };
            CheckValues<float, FloatInterpolator>(from, to, duration, list);
        }

        [Test]
        public void Create2Floats_Test()
        {
            var from = new float2(2, 2);
            var to = new float2(1, 1);
            var duration = 2;
            var axis = new bool2(true, false);
            var list = new List<ProcessId>
            {
                EF.Create(from, to, duration, Function.Linear).GetIdentifier(),
                EF.Create(from, to, duration, new RangedFunction(Function.Linear)).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData()).GetIdentifier(),
                EF.Create(from, to, duration, new Func2(Function.Linear, Function.EaseOutSine), axis).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData(), new GraphData(), axis).GetIdentifier(),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.Loop, 1, (v) => Debug.Log(v)),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.Loop, 1),
            };
            CheckValues<float2, Vector2Interpolator>(from, to, duration, list);
        }

        [Test]
        public void Create3Floats_Test()
        {
            var from = new float3(2, 2, 2);
            var to = new float3(1, 1, 1);
            var duration = 2;
            var axis = new bool3(true, false, true);
            var list = new List<ProcessId> 
            {
                EF.Create(from, to, duration, Function.Linear).GetIdentifier(),
                EF.Create(from, to, duration, new RangedFunction(Function.Linear)).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData()).GetIdentifier(),
                EF.Create(from, to, duration, new Func3(Function.Linear, Function.EaseOutSine, Function.EaseOutBounce), axis).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData(), new GraphData(), new GraphData(), axis).GetIdentifier(),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.Loop, 1, (v) => Debug.Log(v)),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.Loop, 1),
            };
            CheckValues<float3, Vector3Interpolator>(from, to, duration, list);
        }

        [Test]
        public void Create4Floats_Test()
        {
            var from = new float4(2, 2, 2, 2);
            var to = new float4(1, 1, 1, 1);
            var duration = 2;
            var axis = new bool4(true, false, true, true);
            var list = new List<ProcessId>
            {
                EF.Create(from, to, duration, Function.Linear).GetIdentifier(),
                EF.Create(from, to, duration, new RangedFunction(Function.Linear)).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData()).GetIdentifier(),
                EF.Create(from, to, duration, new Func4(Function.Linear, Function.EaseOutSine, Function.EaseOutBounce, Function.Linear), axis).GetIdentifier(),
                EF.Create(from, to, duration, new GraphData(), new GraphData(), new GraphData(), new GraphData(), axis).GetIdentifier(),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.Loop, 1, (v) => Debug.Log(v)),
                EF.CreateNonAlloc(from, to, duration, Function.Linear, TimeControl.Loop, 1),
            };
            CheckValues<float4, Vector4Interpolator>(from, to, duration, list);
        }

        private static void CheckValues<T, D>(T from, T to, float duration, List<ProcessId> ids)
            where T : unmanaged
            where D : IInterpolator<T>
        {
            foreach (var id in ids)
            {
                EFAnimator.TryGetProcessor<T, D>(id, out var proc);
                Assert.That(proc.From.Equals(from) && proc.To.Equals(to) && proc.Clock.Duration == duration &&
                    id.Equals(proc.GetIdentifier()));
            }
        }
    }
}
