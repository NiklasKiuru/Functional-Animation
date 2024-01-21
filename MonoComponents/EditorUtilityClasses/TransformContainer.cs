using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public class TransformContainer<T> where T : VectorContainerBase, new()
    {
        [Tooltip("Position animation properties")]
        [SerializeField] protected T _position = new();

        [Tooltip("Rotation animation properties")]
        [SerializeField] protected T _rotation = new();

        [Tooltip("Scale animation properties")]
        [SerializeField] protected T _scale = new();

        public T Position { get => _position; internal set => _position = value; }
        public T Rotation { get => _rotation; internal set => _rotation = value; }
        public T Scale { get => _scale; internal set => _scale = value; }
    }
}
