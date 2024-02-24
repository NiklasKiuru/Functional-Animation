using UnityEngine;
using System;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class Timeline
    {
        [SerializeField] private Vector2[] _nodes;

        /// <summary>
        /// Timeline values where x represents time and y the starting or ending value of a function
        /// </summary>
        public Vector2[] Nodes { get => _nodes; }

        /// <summary>
        /// Creates a new timeline with the specified starting length
        /// </summary>
        /// <param name="startingLength"></param>
        public Timeline(int startingLength)
        {
            _nodes = new Vector2[startingLength];
        }

        /// <summary>
        /// Creates a new timeline with a default length of 2
        /// </summary>
        public Timeline()
        {
            _nodes = new Vector2[2] { new Vector2(0, 0), new Vector2(1, 1) };
        }

        /// <summary>
        /// Adds a new node into the timeline at the specified index
        /// </summary>
        /// <param name="node"></param>
        /// <param name="index"></param>
        public void AddNode(Vector2 node, int index)
        {
            var newNodes = new Vector2[_nodes.Length + 1];
            var indexer = 0;
            for (int i = 0; i < newNodes.Length; i++)
            {
                if (i == index)
                {
                    newNodes[i] = node;
                    continue;
                }
                newNodes[i] = _nodes[indexer];
                indexer++;
            }
            _nodes = newNodes;
        }

        /// <summary>
        /// Removes a node from the timeline at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveNode(int index)
        {
            var newNodes = new Vector2[_nodes.Length - 1];
            var indexer = 0;
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (i == index)
                    continue;
                newNodes[indexer] = _nodes[i];
                indexer++;
            }
            if (index == 0)
            {
                var node = newNodes[0];
                node.x = 0;
                newNodes[0] = node;
            }
            else if (index == _nodes.Length - 1)
            {
                var node = newNodes[^1];
                node.x = 1;
                newNodes[^1] = node;
            }

            _nodes = newNodes;
        }

        /// <summary>
        /// Moves a node to a new position in the timeline and returns the new valid position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector2 MoveNode(Vector2 pos, int index)
        {
            pos = new Vector2(Mathf.Clamp01(pos.x), Mathf.Clamp(pos.y, -1, 1));
            if (index == 0)
                pos.x = 0;
            else if (index == _nodes.Length - 1)
                pos.x = 1;
            else
            {
                float min = _nodes[index - 1].x;
                float max = _nodes[index + 1].x;
                pos.x = Mathf.Clamp(pos.x, min, max);
            }
            _nodes[index] = pos;
            return pos;
        }
    }
}

