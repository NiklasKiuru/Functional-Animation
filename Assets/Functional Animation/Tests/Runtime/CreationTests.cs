using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using Aikom.FunctionalAnimation.Utility;
using Unity.Burst;
using Unity.Collections;

namespace Aikom.FunctionalAnimation.Tests
{
    public class CreationTests
    {
        [Test]
        public void CreateFloats_Test()
        {
            float from = 2;
            float to = 1;
            float duration = 2;
            var list = new List<IInterpolatorHandle<float, FloatInterpolator>>()
            {
                EF.Create(from, to, new FloatInterpolator(), duration, Function.EaseOutElastic),
                EF.Create(from, to, new FloatInterpolator(), duration, new RangedFunction(Function.EaseOutCirc)),
                EF.Create(from, to, new FloatInterpolator(), duration, new RangedFunction(new FunctionAlias("Test"), float2.zero, new float2(1,1))),
                EF.Create(from, to, new FloatInterpolator(), duration, new RangedFunction(CustomFuncType.CustomFunction, float2.zero, new float2(1,1))),
                EF.Create(from, to, new FloatInterpolator(), duration, new GraphData()),
            };
            CheckValues(from, to, duration, list);
        }

        [Test]
        public void AddPlugin_Test()
        {
            
        }

        [Test]
        public void CreateNativeGraph_Test()
        {
            var managedGraph = new GraphData();
            var nativeGraph = new NativeFunctionGraph(managedGraph, Unity.Collections.Allocator.Persistent);
            try
            {
                Assert.IsTrue(nativeGraph.IsCreated && nativeGraph.Length == 1 && math.all(nativeGraph[0].End == new float2(1, 1)));
            }
            finally
            {
                nativeGraph.Dispose();
            }
        }

        [Test]
        public void CreateNestedGraphHeap_Test()
        {
            var nativeGraphCount = 10;
            var managedGraphs = new GraphData[nativeGraphCount];
            var unManagedGraphs = new NativeArray<NativeFunctionGraph>(nativeGraphCount, Allocator.Persistent);

            try
            {
                for(int i = 0; i < nativeGraphCount; i++)
                {
                    var graph = new GraphData();
                    var nativeGraph = new NativeFunctionGraph(graph, Allocator.Persistent);
                    managedGraphs[i] = graph; 
                    unManagedGraphs[i] = nativeGraph;
                }

                Assert.IsTrue(unManagedGraphs.Length == nativeGraphCount);
            }

            finally 
            { 
                for(int i = 0; i < unManagedGraphs.Length; i++)
                {
                    if (unManagedGraphs[i].IsCreated)
                        unManagedGraphs[i].Dispose();
                }
                unManagedGraphs.Dispose();
            }
        }

        private static void CheckValues<T, D>(T from, T to, float duration, List<IInterpolatorHandle<T, D>> handles)
            where T : unmanaged
            where D : unmanaged, IInterpolator<T>
        {
            foreach (var handle in handles)
            {

            }
        }

        [BurstCompile]
        public class CustomFuncType
        {
            [BurstCompile]
            [EFunction("Test")]
            public static float CustomFunction(float x)
            {
                return x * 2;
            }
        }
    }
}
