using Aikom.FunctionalAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MultiTargetContoller
{
    private Interpolator<Vector3>[] _vectorInterpolators = new Interpolator<Vector3>[3]
    {
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero),
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero),
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero)
    };
    private MatrixRxC<bool> _animationChecks;
    private Transform _target;
    private bool _isActive;
    private TransformAnimation _data;
    private TransformGroup _transformGroup;

    internal Interpolator<Vector3>[] VectorInterpolators { get => _vectorInterpolators; }

    /// <summary>
    /// Creates all interpolators for the current animation and activates the controller
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="target"></param>
    internal void SetAnimation(TransformAnimation anim, Transform target, List<Transform> group)
    {
        _isActive = true;
        _data = anim;
        _animationChecks = anim.GetSelectionMatrix();
        _target = target;
        _transformGroup = new TransformGroup(target, group);
        _vectorInterpolators = new Interpolator<Vector3>[3];
        var properties = (TransformProperty[])Enum.GetValues(typeof(TransformProperty));
        for (int i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            var data = anim[prop];

            // Do not animate this property
            if (!data.Animate)
            {
                _vectorInterpolators[i] = null;
                continue;
            }

            Action<Vector3> setValFunc;
            Func<float, Vector3, Vector3, Vector3> incrimentFunc;

            // Animate all axis
            if (_animationChecks[i, 3])
            {
                var func = anim[prop].GenerateFunction(Axis.W);
                incrimentFunc = IncrimentAll(func);
                setValFunc = DefineSetValueFunctionAll(prop);
            }
            else
            {
                setValFunc = DefineSetValueFunctionSeparate(prop, data.AnimateableAxis);
                var funcs = new Func<float, float>[3];
                for (int j = 0; j < data.Length - 1; j++)
                {
                    if (!_animationChecks[i, j])
                        continue;
                    funcs[j] = anim[prop].GenerateFunction((Axis)j);
                }
                incrimentFunc = IncrimentSelected(funcs, prop);

            }

            var start = data.Mode == AnimationMode.Relative ? GetPropValue(prop) : data.Start;
            var end = data.Mode == AnimationMode.Relative ? GetPropValue(prop) + data.Offset : data.Target;

            var duration = data.Sync ? anim.Duration : data.Duration;
            _vectorInterpolators[i] = new Interpolator<Vector3>(incrimentFunc, setValFunc,
                1 / duration, start, end, data.TimeControl);
        }
    }

    /// <summary>
    /// Updates the current animation
    /// </summary>
    internal void Update()
    {
        if (!_isActive)
            return;
        int activeCount = 0;
        for (int i = 0; i < _vectorInterpolators.Length; i++)
        {
            var interpolator = _vectorInterpolators[i];
            if (interpolator == null || interpolator.InternalState == Interpolator<Vector3>.Status.Stopped)
                continue;

            interpolator.Run();
            activeCount++;
        }
        _isActive = activeCount > 0;
    }

    /// <summary>
    /// Resets the current animation
    /// </summary>
    internal void ResetCurrent()
    {
        for (int i = 0; i < _vectorInterpolators.Length; i++)
        {
            var interpolator = _vectorInterpolators[i];
            if (interpolator == null)
                continue;
            interpolator.Reset();
        }
    }

    /// <summary>
    /// Resets current property values into initial state
    /// </summary>
    /// <param name="prop"></param>
    internal void ResetProperty(TransformProperty prop)
    {
        if (_vectorInterpolators[(int)prop] == null)
            return;
        _vectorInterpolators[(int)prop].Reset();
    }

    /// <summary>
    /// Overrides the runtime target value for a specific axis and property
    /// If selected axis is the final axis, the value will be applied to all three dimensional axis
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="axis"></param>
    /// <param name="value"></param>
    public void OverrideTarget(TransformProperty prop, Axis axis, float value)
    {
        if (!_isActive || _animationChecks[(int)prop, (int)axis])
        {
            if (axis == Axis.W)
            {
                var vec = new Vector3(value, value, value);
                OverrideTarget(prop, vec);
                return;
            }
            var current = _vectorInterpolators[(int)prop].Target;
            current[(int)axis] = value;
            _vectorInterpolators[(int)prop].OverrideTarget(current, false);
        }
    }

    /// <summary>
    /// Overrides the runtime target value for a specific property as long as there is no axis separation
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="value"></param>
    public void OverrideTarget(TransformProperty prop, Vector3 value)
    {
        if (!_isActive || _animationChecks[(int)prop, (int)Axis.W])
            return;
        _vectorInterpolators[(int)prop].OverrideTarget(value, false);
    }

    private Func<float, Vector3, Vector3, Vector3> IncrimentAll(Func<float, float> ease)
    {
        return (t, start, end) => EF.Interpolate(ease, start, end, t);
    }

    private Func<float, Vector3, Vector3, Vector3> IncrimentSelected(Func<float, float>[] easingFuncs, TransformProperty prop)
    {
        var row = _animationChecks.GetRow((int)prop);
        return (t, start, end) =>
        {
            var vector = new Vector3();
            for (int i = 0; i < easingFuncs.Length; i++)
            {
                if (!row[i])
                    continue;
                vector[i] = EF.Interpolate(easingFuncs[i], start[i], end[i], t);
            }
            return vector;
        };
    }

    private Vector3 GetPropValue(TransformProperty prop)
    {
        return prop switch
        {
            TransformProperty.Position => _target.localPosition,
            TransformProperty.Rotation => _target.localRotation.eulerAngles,
            TransformProperty.Scale => _target.localScale,
            _ => throw new System.NotImplementedException(),
        };
    }

    private Action<Vector3> DefineSetValueFunctionSeparate(TransformProperty prop, bool3 animateableAxis)
    {
        switch (prop)
        {
            case TransformProperty.Position:
                return (v) =>
                {
                    var vector = new Vector3(animateableAxis.x ? v.x : _target.localPosition.x,
                        animateableAxis.y ? v.y : _target.localPosition.y,
                        animateableAxis.z ? v.z : _target.localPosition.z);
                    _target.localPosition = vector;
                    for(int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        var child = _transformGroup.Children[i];
                        child.Target.localPosition = child.PositionOffset + vector;
                    }
                };
            case TransformProperty.Rotation:
                return (v) =>
                {
                    var vector = new Vector3(animateableAxis.x ? v.x : _target.localRotation.eulerAngles.x,
                        animateableAxis.y ? v.y : _target.localRotation.eulerAngles.y,
                        animateableAxis.z ? v.z : _target.localRotation.eulerAngles.z);
                    _target.localRotation = Quaternion.Euler(vector);
                    for (int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        var child = _transformGroup.Children[i];
                        child.Target.localRotation = Quaternion.Euler(child.RotationOffset + vector);
                    }
                };
            case TransformProperty.Scale:
                return (v) =>
                {
                    var vector = new Vector3(animateableAxis.x ? v.x : _target.localScale.x,
                        animateableAxis.y ? v.y : _target.localScale.y,
                        animateableAxis.z ? v.z : _target.localScale.z);
                    _target.localScale = vector;
                    for (int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        var child = _transformGroup.Children[i];
                        child.Target.localScale = child.ScaleOffset + vector;
                    }
                };
        }
        throw new System.NotImplementedException();
    }

    private Action<Vector3> DefineSetValueFunctionAll(TransformProperty prop)
    {
        switch (prop)
        {
            case TransformProperty.Position:
                return (v) => 
                { 
                    _target.localPosition = v;
                    for (int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        var child = _transformGroup.Children[i];
                        child.Target.localPosition = child.PositionOffset + v;
                    }
                };
            case TransformProperty.Rotation:
                return  (v) =>
                { 
                    _target.localRotation = Quaternion.Euler(v);
                    for (int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        var child = _transformGroup.Children[i];
                        child.Target.localRotation = Quaternion.Euler(child.RotationOffset + v);
                    }
                };
            case TransformProperty.Scale:
                return (v) => 
                { 
                    _target.localScale = v;
                    for (int i = 0; i < _transformGroup.Children.Count; i++)
                    {
                        var child = _transformGroup.Children[i];
                        child.Target.localScale = child.ScaleOffset + v;
                    }
                };
        };
        throw new System.NotImplementedException();
    }
}

public class TransformGroup
{   
    public Transform Parent { get; private set; }
    public List<TransformChild> Children { get; private set; }
    
    public TransformGroup(Transform parent, List<Transform> group)
    {
        Parent = parent;
        Children = new List<TransformChild>();
        foreach (var item in group)
        {
            var child = new TransformChild();
            child.Target = item;
            child.PositionOffset = item.localPosition - parent.localPosition;
            child.RotationOffset = item.localRotation.eulerAngles - parent.localRotation.eulerAngles;
            child.ScaleOffset = item.localScale - parent.localScale;
            Children.Add(child);
        }
    }

    public struct TransformChild
    {   
        public Transform Target;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        public Vector3 ScaleOffset;
    }
}
