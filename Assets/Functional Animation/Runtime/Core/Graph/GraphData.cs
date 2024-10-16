using Codice.Client.BaseCommands;
using System;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class GraphData : ICloneable
    {
        [SerializeField] private FunctionAlias[] _functions = new FunctionAlias[1];
        [SerializeField] private Timeline _timeline;

        public IReadOnlyCollection<FunctionAlias> Functions { get => _functions; }
        public IReadOnlyCollection<Vector2> Nodes { get => _timeline.Nodes; }
        public int Length 
        { 
            get
            {
                if (_functions != null)
                    return _functions.Length;
                return 0;
            }
        }

        /// <summary>
        /// Creates new graph data with a given function as the default first function
        /// </summary>
        /// <param name="func"></param>
        public GraphData(Function func = Function.Linear)
        {
            _functions[0] = new FunctionAlias(func);
            _timeline = new Timeline();
        }

        public void CopyData(ref Span<RangedFunction> span)
        {
            int max = Mathf.Min(span.Length, _functions.Length);
            for (int i = 0; i < max; ++i)
            {
                span[i] = new RangedFunction
                {
                    Pointer = BurstFunctionCache.GetCachedPointer(_functions[i]),
                    Start = _timeline.Nodes[i],
                    End = _timeline.Nodes[i + 1]
                };
            }
        }

        /// <summary>
        /// Evaluates the graph at given time point
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Evaluate(float time)
        {
            var count = _functions.Length;
            for (int i = 0; i < count; i++)
            {
                var thisNode = _timeline.Nodes[i];
                var nextNode = _timeline.Nodes[i + 1];
                if (time >= thisNode.x && time < nextNode.x)
                {
                    var rFunc = new RangedFunction(_functions[i], thisNode, nextNode);
                    return rFunc.Evaluate(time);
                }
            }
            var func = new RangedFunction(_functions[count - 1], _timeline.Nodes[count - 1], _timeline.Nodes[count]);
            return func.Evaluate(time);
        }

        /// <summary>
        /// Returns a ranged function array of the functions in this graph data. The array is always the max function buffer size defined in <see cref="EFSettings"/> 
        /// </summary>
        /// <returns></returns>
        public RangedFunction[] GetRangedFunctionArray()
        {   
            var array = new RangedFunction[EFSettings.MaxFunctions];
            GetRangedFunctionsNonAlloc(ref array);
            return array;
        }

        /// <summary>
        /// Gets the ranged functions stored in this graph without allocating a new array
        /// </summary>
        /// <remarks>This function does not guarantee that the index stays inside the given array</remarks>
        /// <param name="arr"></param>
        public void GetRangedFunctionsNonAlloc(ref RangedFunction[] arr, int startingIndex = 0)
        {
            int max = Mathf.Min(arr.Length, _functions.Length);
            for(int i = 0; i < max; ++i)
            {
                arr[i + startingIndex] = new RangedFunction 
                {
                    Pointer = BurstFunctionCache.GetCachedPointer(_functions[i]),
                    Start = _timeline.Nodes[i],
                    End = _timeline.Nodes[i + 1]
                };
            }
        }

        /// <summary>
        /// Adds a function to the function array and readjusts the timeline based on added function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="pos"></param>
        public void AddFunction(FunctionAlias function, Vector2 pos)
        {
            var newArray = new FunctionAlias[_functions.Length + 1];
            var timeline = _timeline;

            pos = new Vector2(Mathf.Clamp01(pos.x), Mathf.Clamp(pos.y, -1, 1));
            var index = 0;
            for (int i = 0; i < _functions.Length; i++)
            {
                var node = timeline.Nodes[i];
                if (pos.x >= node.x && pos.x <= timeline.Nodes[i + 1].x)
                {
                    index = i + 1;
                    break;
                }
            }

            timeline.AddNode(pos, index);
            var indexer = 0;
            for (int i = 0; i < newArray.Length; i++)
            {
                if (i == index - 1)
                {
                    newArray[i] = function;
                    continue;
                }
                newArray[i] = _functions[indexer];
                ++indexer;
            }

            _functions = newArray;
        }

        /// <summary>
        /// Removes a function from the function array and readjusts the timeline based on removed function
        /// </summary>
        /// <param name="index"></param>
        public bool RemoveFunction(int index)
        {   
            if(_functions.Length == 1)
            {
                Debug.LogWarning("Cannot remove the only function in the array");
                return false;
            }
            var newArray = new FunctionAlias[_functions.Length - 1];
            var timeline = _timeline;

            timeline.RemoveNode(index);
            var indexer = 0;
            for (int i = 0; i < _functions.Length; i++)
            {
                if (i == index)
                    continue;
                newArray[indexer] = _functions[i];
                indexer++;
            }
            _functions = newArray;
            return true;
        }

        /// <summary>
        /// Moves a timeline node to a new position and returns the new valid position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Vector2 MoveTimelineNode(int position, Vector2 value)
        {
            return _timeline.MoveNode(value, position);
        }

        /// <summary>
        /// Changes the function at a given index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="func"></param>
        public void ChangeFunction(int index, FunctionAlias func)
        {
            _functions[index] = func;
        }

        public object Clone()
        {
            var cloneGraph = new GraphData();
            cloneGraph._functions = (FunctionAlias[])_functions.Clone();
            cloneGraph._timeline = new Timeline(Length + 1);
            for(int i = 0; i < _functions.Length + 1; ++i)
            {
                cloneGraph._timeline.Nodes[i] = _timeline.Nodes[i];
            }
            return cloneGraph;
        }
    }
}

