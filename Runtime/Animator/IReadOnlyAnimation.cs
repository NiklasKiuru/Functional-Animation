using Aikom.FunctionalAnimation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IReadOnlyAnimation
{
    public IReadOnlyCollection<AnimationData> Data { get; }
}
