using Aikom.FunctionalAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class ScriptableTransformAnimator : TransformAnimator
    {
        [SerializeField] private TransformAnimation _animation;

        public void Load(TransformAnimation anim)
        {
            Container.Position = anim.Data.Position;
            Container.Rotation = anim.Data.Rotation;
            Container.Scale = anim.Data.Scale;
            SyncAll = anim.Sync;
            Loop = anim.Loop;
            MaxDuration = anim.Duration;

            Validate();
        }

        protected override void OnValidate()
        {   
            if(_animation != null)
            {
                Load(_animation);
            }
        }
    }
}

