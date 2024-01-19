using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class TransformGraphAnimator : AnimatorBase<Vector3>
    {
        [Tooltip("Position animation properties")]
        [SerializeField] private VectorGraphContainer _position = new();

        [Tooltip("Rotation animation properties")]
        [SerializeField] private VectorGraphContainer _rotation = new();

        [Tooltip("Scale animation properties")]
        [SerializeField] private VectorGraphContainer _scale = new();

        private VectorGraphContainer[] _activeTargets = new VectorGraphContainer[3];

        public override PropertyContainer<Vector3>[] ActiveTargets
        {
            get => _activeTargets;
            protected set => _activeTargets = (VectorGraphContainer[])value;
        }

        /// <summary>
        /// Pauses or unpauses the animation of a transform property
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="pause"></param>
        public void Pause(TransformProperty prop, bool pause)
        {
            Pause((int)prop, pause);
        }

        protected override void SetTargets()
        {
            _position.CreateInterpolator(transform.position, (p) => { transform.position = p; });
            _activeTargets[0] = _position;
            _rotation.CreateInterpolator(transform.rotation.eulerAngles, (r) => { transform.rotation = Quaternion.Euler(r); });
            _activeTargets[1] = _rotation;
            _scale.CreateInterpolator(transform.localScale, (s) => { transform.localScale = s; });
            _activeTargets[2] = _scale;
        }
    }

}

