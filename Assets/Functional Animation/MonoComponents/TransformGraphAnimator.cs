using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class TransformGraphAnimator : TransformAnimatorBase<TransformContainer<VectorGraphContainer>, VectorGraphContainer>
    {
        // Since unity does not support generic monobehaviours, there has to exist a concrete implementation of the generic TransformAnimatorBase class.
        // All the logic required what i wanted to acheive is achieved in the abstract class level, so this class is empty.
    }

}

