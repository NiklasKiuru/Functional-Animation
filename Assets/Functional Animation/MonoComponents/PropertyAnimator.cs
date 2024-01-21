using System;
using UnityEngine;
using UnityEngine.Events;

namespace Aikom.FunctionalAnimation
{
    public class PropertyAnimator : AnimatorBase<float>
    {
        [SerializeField] private FloatContainer[] _activeTargets = new FloatContainer[0];

        public override PropertyContainer<float>[] ActiveTargets 
        { 
            get => _activeTargets;
        }

        protected override void SetTargets()
        {
            foreach (var target in _activeTargets)
            {
                target.CreateInterpolator(target.StartValue, (v) => { target.OnValueChangedEvent.Invoke(v); });
            }
        }
    }
}
