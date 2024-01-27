using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Container for used editor functions
    /// </summary>
    [Serializable]
    public class FunctionConstructor : ICloneable
    {
        private FunctionWrapper[] _functions = new FunctionWrapper[0];
        [SerializeField] private GraphData[] _functionData = new GraphData[0];

        public GraphData[] FunctionData { get => _functionData; set => _functionData = value; }

        /// <summary>
        /// Lenght of the function array this wrapper generates the final function from
        /// </summary>
        public int Length { get => _functions.Length; }

        /// <summary>
        /// Implimentation of the ICloneable interface
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var copy = new FunctionConstructor();
            copy._functions = new FunctionWrapper[_functions.Length];
            for(int i = 0; i < _functions.Length; i++)
            {
                copy._functions[i] = new FunctionWrapper();
                copy._functions[i]._function = _functions[i]._function;
                copy._functions[i]._invert = _functions[i]._invert;
            }
            return copy;
        }

        /// <summary>
        /// Generates a function from the selected functions in the editor. If the array is empty, a linear function is returned
        /// </summary>
        /// <returns></returns>
        public Func<float, float> GenerateSimple()
        {
            var fallback = CreateFallback();
            if (fallback != null)
                return fallback;


            var funcs = new Func<float, float>[_functions.Length];

            for(int i = 0; i < _functions.Length; i++)
            {
                if (_functions[i]._invert)
                    funcs[i] = EF.Invert(EditorFunctions.Funcs[_functions[i]._function]);
                else
                    funcs[i] = EditorFunctions.Funcs[_functions[i]._function];
            }

            return EF.Combine(funcs);
        }

        public Func<float, float> GenerateComplex()
        {
            if(_functions.Length < 2)
                return EditorFunctions.Funcs[Function.Linear];

            var funcs = new Func<float, float>[_functions.Length];
            var timeLine = new float[_functions.Length * 4];

            for(int i = 0; i < _functions.Length; i++)
            {
                var baseFunc = EditorFunctions.Funcs[_functions[i]._function];
                var endingValue = _functions[i]._endingValue;
                var startingValue = _functions[i]._startingValue;
                var amplitude = endingValue - startingValue;
                var startingPoint = _functions[i]._startingPoint;
                var endingPoint = _functions[i]._endingPoint;
                var totalTime = 1 - (1 - endingPoint) - startingPoint;
                

                Func<float, float> finalFunc = (t) =>
                {
                    if (t < startingPoint)
                        return startingValue;
                    else if (t < endingPoint)
                    {   
                        var time = (t - startingPoint) * (1 / totalTime);
                        return baseFunc(time) * amplitude + startingValue;
                    }
                        
                    else
                        return endingValue;
                };

                funcs[i] = finalFunc;
            }

            return EF.Combine(funcs);
        }

        private Func<float, float> CreateFallback()
        {
            if (_functions.Length == 0)
                return EditorFunctions.Funcs[Function.Linear];
            if (_functions.Length == 1)
            {
                var func = EditorFunctions.Funcs[_functions[0]._function];
                if (_functions[0]._invert)
                    return EF.Invert(func);
                else
                    return EditorFunctions.Funcs[_functions[0]._function];
            }
            
            return null;
        }

        [Serializable]
        public class FunctionWrapper
        {
            [Tooltip("Used function type")]
            [SerializeField] internal Function _function;

            [Tooltip("Uninverted functions always start with value v = 0 at their respective linear time t = 0 and inverted functions return 1 - v at any time t")]
            [SerializeField] internal bool _invert;

            [SerializeField] internal float _startingValue = 0;
            [SerializeField] internal float _endingValue = 1;

            // The point in which the function is called with value >= 0
            [SerializeField][Range(0,1)] internal float _startingPoint = 0;

            // The point in which the function is called with value 1
            [SerializeField][Range(0,1)] internal float _endingPoint = 1;
        }
    }

    [Serializable]
    public class GraphData
    {
        public Function[] Functions { get; private set; } = new Function[1];
        public Timeline Timeline;

        /// <summary>
        /// Creates new graph data with a given function as the default first function
        /// </summary>
        /// <param name="func"></param>
        public GraphData(Function func = Function.Linear)
        {
            Functions[0] = func;
            Timeline = new Timeline();
        }

        /// <summary>
        /// Generates a function that modulates a value with this data based on given input
        /// </summary>
        /// <returns></returns>
        public Func<float, float> GenerateFunction()
        {
            var funcs = new Func<float, float>[Functions.Length];
            for (int i = 0; i < Functions.Length; i++)
            {
                var startingNode = Timeline.Nodes[i];
                var endingNode = Timeline.Nodes[i + 1];
                var baseFunc = EditorFunctions.Funcs[Functions[i]];
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
                float count = Functions.Length;
                for (int i = 0; i < count; i++)
                {
                    var node = Timeline.Nodes[i];
                    if (t >= Timeline.Nodes[i].x && t < Timeline.Nodes[i + 1].x)
                    {
                        return funcs[i](t);
                    }
                }
                return funcs[(int)count - 1](t);
            };
        }

        /// <summary>
        /// Adds a function to the function array and readjusts the timeline based on added function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="pos"></param>
        public void AddFunction(Function function, Vector2 pos)
        {
            var newArray = new Function[Functions.Length + 1];
            var timeline = Timeline;

            pos = new Vector2(Mathf.Clamp01(pos.x), Mathf.Clamp01(pos.y));
            var index = 0;
            for (int i = 0; i < Functions.Length; i++)
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
                newArray[i] = Functions[indexer];
                ++indexer;
            }

            Functions = newArray;
        }

        /// <summary>
        /// Removes a function from the function array and readjusts the timeline based on removed function
        /// </summary>
        /// <param name="index"></param>
        public void RemoveFunction(int index)
        {
            var newArray = new Function[Functions.Length - 1];
            var timeline = Timeline;

            timeline.RemoveNode(index);
            var indexer = 0;
            for (int i = 0; i < Functions.Length; i++)
            {
                if (i == index)
                    continue;
                newArray[indexer] = Functions[i];
                indexer++;
            }
            Functions = newArray;
        }

        /// <summary>
        /// Moves a timeline node to a new position and returns the new valid position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Vector2 MoveTimelineNode(int position, Vector2 value)
        {
            return Timeline.MoveNode(value, position);
        }
    }
}
