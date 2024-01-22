using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [DisallowMultipleComponent]
    public abstract class TransformAnimatorBase<T, D> : AnimatorBase<Vector3> 
        where T : TransformContainer<D>, new() 
        where D : VectorContainerBase, new()
    {
        [Tooltip("Transform animation properties")]
        [SerializeField] private T _container = new();

        internal T Container { get => _container; }
        public sealed override PropertyContainer<Vector3>[] ActiveTargets { get; } = new PropertyContainer<Vector3>[3];

        /// <summary>
        /// Pauses or unpauses the animation of a transform property
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="pause"></param>
        public void Pause(TransformProperty prop, bool pause)
        {
            Pause((int)prop, pause);
        }

        protected sealed override void SetTargets()
        {
            _container.Position.SetNewTarget(_container.Position.Offset + transform.position);
            _container.Position.CreateInterpolator(transform.position, (p) => { transform.position = p; });

            _container.Rotation.SetNewTarget(_container.Rotation.Offset + transform.rotation.eulerAngles);
            _container.Rotation.CreateInterpolator(transform.rotation.eulerAngles, (r) => { transform.rotation = Quaternion.Euler(r); });

            _container.Scale.SetNewTarget(_container.Scale.Offset + transform.localScale);
            _container.Scale.CreateInterpolator(transform.localScale, (s) => { transform.localScale = s; });

            for(int i = 0; i < 3; i++)
            {
                ActiveTargets[i] = _container[i];
            }
        }
    }

    public enum TransformProperty
    {
        Position = 0,
        Rotation = 1,
        Scale = 2
    }
}
