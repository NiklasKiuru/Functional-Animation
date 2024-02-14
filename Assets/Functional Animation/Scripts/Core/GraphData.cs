using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class GraphData
    {
        [SerializeField] private Function[] _functions = new Function[1];
        [SerializeField] private Timeline _timeline;

        public IReadOnlyCollection<Function> Functions { get => _functions; }
        public IReadOnlyCollection<Vector2> Nodes { get => _timeline.Nodes; }
        public int Length { get => _functions.Length; }

        /// <summary>
        /// Creates new graph data with a given function as the default first function
        /// </summary>
        /// <param name="func"></param>
        public GraphData(Function func = Function.Linear)
        {
            _functions[0] = func;
            _timeline = new Timeline();
        }

        /// <summary>
        /// Generates a function that modulates a value with this data based on given input
        /// </summary>
        /// <returns></returns>
        public Func<float, float> GenerateFunction()
        {
            var funcs = new Func<float, float>[_functions.Length];
            for (int i = 0; i < _functions.Length; i++)
            {
                var startingNode = _timeline.Nodes[i];
                var endingNode = _timeline.Nodes[i + 1];
                var baseFunc = EditorFunctions.Funcs[_functions[i]];
                var endingValue = endingNode.y;
                var startingValue = startingNode.y;
                var amplitude = endingValue - startingValue;
                var startingPoint = startingNode.x;
                var endingPoint = endingNode.x;
                var totalTime = 1 - (1 - endingPoint) - startingPoint;

                Func<float, float> finalFunc = (t) =>
                {
                    var time = (t - startingPoint) * (1 / totalTime);
                    return baseFunc(time) * amplitude + startingValue;
                };

                funcs[i] = finalFunc;
            }

            return (t) =>
            {
                float count = _functions.Length;
                for (int i = 0; i < count; i++)
                {
                    if (t >= _timeline.Nodes[i].x && t < _timeline.Nodes[i + 1].x)
                    {
                        return funcs[i](t);
                    }
                }
                return funcs[(int)count - 1](t);
            };
        }

        /// <summary>
        /// Returns a pointer array of the functions in this graph data
        /// </summary>
        /// <returns></returns>
        public RangedFunction[] GetRangedFunctionArray()
        {   
            var array = new RangedFunction[_functions.Length];
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
            for(int i = 0; i < _functions.Length; ++i)
            {
                arr[i + startingIndex] = new RangedFunction 
                {
                    Pointer = EditorFunctions.Pointers[_functions[i]],
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
        public void AddFunction(Function function, Vector2 pos)
        {
            var newArray = new Function[_functions.Length + 1];
            var timeline = _timeline;

            pos = new Vector2(Mathf.Clamp01(pos.x), Mathf.Clamp01(pos.y));
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
            var newArray = new Function[_functions.Length - 1];
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
        public void ChangeFunction(int index, Function func)
        {
            _functions[index] = func;
        }
    }
}

