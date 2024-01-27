using Aikom.FunctionalAnimation.Utility;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class AnimationData : IIndexable<GraphData>
    {
        // X-Axis modulation
        public GraphData X;

        // Y-Axis modulation
        public GraphData Y;

        // Z-Axis modulation
        public GraphData Z;

        // Unseparated axis modulation
        public GraphData W;

        public Vector3 Target;
        public Vector3 Offset;
        public bool Animate;
        public bool Sync;
        public bool SeparateAxis;
        public float Duration;
        public TimeControl TimeControl;
        public bool3 AnimateableAxis;

        public int Length => 4;

        public GraphData this[int index]
        {
            get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    3 => W,
                    _ => throw new System.IndexOutOfRangeException(),
                };
            }

            private set
            {
                switch (index)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }

        public AnimationData()
        {
            X = new GraphData();
            Y = new GraphData();
            Z = new GraphData();
            W = new GraphData();
            AnimateableAxis = new bool3(true, true, true);
            Animate = true;
        }

        /// <summary>
        /// Generates an axis modulation function based on the selected axis
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public Func<float, float> GenerateFunction(Axis axis)
        {
            var data = this[(int)axis];
            return data.GenerateFunction();
        }

        /// <summary>
        /// Adds a function to an axis specified data container
        /// </summary>
        /// <param name="function"></param>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        public void AddFunction(Function function, Axis axis, Vector2 pos)
        {
            var data = this[(int)axis];
            data.AddFunction(function, pos);
        }

        /// <summary>
        /// Change timeline data based on node index and axis
        /// </summary>
        /// <param name="axis">Target axis to set the data to</param>
        /// <param name="position">This is the index of the timeline node, not the position of a function</param>
        /// <param name="value"></param>
        /// <returns>Resulting value in the data modified</returns>
        public Vector2 MoveTimelineNode(Axis axis, int position, Vector2 value)
        {   
            var data = this[(int)axis];
            return data.MoveTimelineNode(position, value);
        }

        /// <summary>
        /// Remove function from specified axis data container
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="position">The index position of the target function</param>
        public void RemoveFunction(Axis axis, int position)
        {
            var data = this[(int)axis];
            data.RemoveFunction(position);
        }
    }

}
