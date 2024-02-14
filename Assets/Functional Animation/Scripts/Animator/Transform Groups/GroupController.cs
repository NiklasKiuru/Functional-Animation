using Aikom.FunctionalAnimation;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class GroupController : IDisposable
{
    private Interpolator<Vector3>[] _vectorInterpolators = new Interpolator<Vector3>[3]
    {
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero),
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero),
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero)
    };
    private MatrixRxC<bool> _animationChecks;
    private bool3x4 _jobAxisChecks;
    private Transform _target;
    private bool _isActive;
    private bool _hasInactiveGroupChildren;
    private TransformAnimation _data;
    private TransformGroup _addGroup;
    private TransformGroup _updateGroup;
    private List<int> _removeIds;
    private SyncTransformGroupJob _job;
    private NativeList<float3x3> _offsets;
    private float3x3 _currentValues;
    private float3x3 _originValues;
    private TransformAccessArray _transformAccessArray;
    private JobHandle _jobHandle;
    private int _threadCount;

    internal Interpolator<Vector3>[] VectorInterpolators { get => _vectorInterpolators; }
    public bool IsActive { get => _isActive; }

    public GroupController(int threadCount)
    {
        _addGroup = new TransformGroup(null, new List<Transform>());
        _removeIds = new List<int>();
        _threadCount = threadCount;
    }

    /// <summary>
    /// Creates all interpolators for the current animation and activates the controller
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="target"></param>
    internal void SetAnimation(TransformAnimation anim, Transform target, List<Transform> group)
    {   
        TransformAccessArray.Allocate(group.Count, _threadCount, out _transformAccessArray);
        for(int i = 0; i < group.Count; i++)
        {
            _transformAccessArray.Add(group[i]);
        }
        _offsets = new NativeList<float3x3>(group.Count, Allocator.Persistent);
        _currentValues = new float3x3();
        _originValues = new float3x3();
        _updateGroup = new TransformGroup(target, group);

        for(int i = 0; i < group.Count; i++)
        {
            var child = group[i];
            var pos = child.localPosition - target.localPosition;
            var rot = child.localRotation.eulerAngles - target.localRotation.eulerAngles;
            var scale = child.localScale - target.localScale;
            _offsets.Add(new float3x3(pos, rot, scale));
        }

        _animationChecks = anim.GetSelectionMatrix();
        var x = _animationChecks.GetColumn(0);
        var boolx = new bool3(x[0], x[1], x[2]);
        var y = _animationChecks.GetColumn(1);
        var booly = new bool3(y[0], y[1], y[2]);
        var z = _animationChecks.GetColumn(2);
        var boolz = new bool3(z[0], z[1], z[2]);
        var w = _animationChecks.GetColumn(3);
        var boolw = new bool3(w[0], w[1], w[2]);
        _jobAxisChecks = new bool3x4(boolx, booly, boolz, boolw);
        _job = new SyncTransformGroupJob()
        {
            Offsets = _offsets,
            CurrentValues = _currentValues,
            OriginValues = _originValues,
            AxisCheck = _jobAxisChecks,
        };

        _isActive = true;
        _data = anim;
        _target = target;
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
    /// Completes the current job if it is running, disposes all unmanaged containers and marks the controller as inactive
    /// </summary>
    public void Disable()
    {
        _isActive = false;
        if(!_jobHandle.IsCompleted)
            _jobHandle.Complete();
        Dispose();
    }

    /// <summary>
    /// Adds a new child to the current group
    /// </summary>
    /// <param name="target"></param>
    internal void AddGroupChild(Transform target)
    {
        _addGroup.Children.Add(new TransformGroup.TransformChild()
        {
            Target = target,
            PositionOffset = target.localPosition - _target.localPosition,
            RotationOffset = Vector3.zero, //target.localRotation.eulerAngles - _target.localRotation.eulerAngles,
            ScaleOffset = Vector3.zero //(target.localScale - _target.localScale
        }) ;
        _hasInactiveGroupChildren = true;
    }

    /// <summary>
    /// Removes a child from the current group
    /// </summary>
    /// <param name="target"></param>
    public void RemoveGroupChild(Transform target)
    {   
        var id = _updateGroup.RemoveChild(target);
        if(id != -1)
            _removeIds.Add(id);
        _hasInactiveGroupChildren = true;
    }

    private void CheckInactiveGroupChildren()
    {
        if (!_hasInactiveGroupChildren)
            return;
        if(_removeIds.Count > 0)
        {
            for (int i = 0; i < _removeIds.Count; i++)
            {   
                var id = _removeIds[i];
                _transformAccessArray.RemoveAtSwapBack(id);
                _offsets.RemoveAtSwapBack(id);
            }
            _removeIds.Clear();
        }
        if (_addGroup.Children.Count > 0)
        {
            for (int i = 0; i < _addGroup.Children.Count; i++)
            {
                var child = _addGroup.Children[i];
                _transformAccessArray.Add(child.Target);
                _offsets.Add(new float3x3(child.PositionOffset, child.RotationOffset, child.ScaleOffset));
                _updateGroup.Children.Add(child);
            }
            _addGroup.Children.Clear();
        }
        _hasInactiveGroupChildren = false;
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
            _currentValues[i] = interpolator.CurrentValue;
            activeCount++;
        }
        _job = new SyncTransformGroupJob()
        {
            Offsets = _offsets,
            CurrentValues = _currentValues,
            OriginValues = _originValues,
            AxisCheck = _jobAxisChecks,
        };

        _jobHandle = _job.Schedule(_transformAccessArray);
        _isActive = activeCount > 0;
    }

    /// <summary>
    /// Completes the current jobhandle. Must be called in late update
    /// </summary>
    internal void ApplyTransformations()
    {
        _jobHandle.Complete();
        CheckInactiveGroupChildren();
    }

    /// <summary>
    /// Disposes unmanaged memeory and destroys the virtual parent
    /// </summary>
    public void Dispose()
    {
        if(_offsets.IsCreated) _offsets.Dispose();
        if(_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        if(_target != null)
            UnityEngine.Object.Destroy(_target.gameObject);
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
                    //for(int i = 0; i < _transformGroup.Children.Count; i++)
                    //{
                    //    var child = _transformGroup.Children[i];
                    //    child.Target.localPosition = child.PositionOffset + vector;
                    //}
                };
            case TransformProperty.Rotation:
                return (v) =>
                {
                    var vector = new Vector3(animateableAxis.x ? v.x : _target.localRotation.eulerAngles.x,
                        animateableAxis.y ? v.y : _target.localRotation.eulerAngles.y,
                        animateableAxis.z ? v.z : _target.localRotation.eulerAngles.z);
                    _target.localRotation = Quaternion.Euler(vector);
                    //for (int i = 0; i < _transformGroup.Children.Count; i++)
                    //{
                    //    var child = _transformGroup.Children[i];
                    //    child.Target.localRotation = Quaternion.Euler(child.RotationOffset + vector);
                    //}
                };
            case TransformProperty.Scale:
                return (v) =>
                {
                    var vector = new Vector3(animateableAxis.x ? v.x : _target.localScale.x,
                        animateableAxis.y ? v.y : _target.localScale.y,
                        animateableAxis.z ? v.z : _target.localScale.z);
                    _target.localScale = vector;
                    //for (int i = 0; i < _transformGroup.Children.Count; i++)
                    //{
                    //    var child = _transformGroup.Children[i];
                    //    child.Target.localScale = child.ScaleOffset + vector;
                    //}
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
                    //for (int i = 0; i < _transformGroup.Children.Count; i++)
                    //{
                    //    var child = _transformGroup.Children[i];
                    //    child.Target.localPosition = child.PositionOffset + v;
                    //}
                };
            case TransformProperty.Rotation:
                return  (v) =>
                { 
                    _target.localRotation = Quaternion.Euler(v);
                    //for (int i = 0; i < _transformGroup.Children.Count; i++)
                    //{
                    //    var child = _transformGroup.Children[i];
                    //    child.Target.localRotation = Quaternion.Euler(child.RotationOffset + v);
                    //}
                };
            case TransformProperty.Scale:
                return (v) => 
                { 
                    _target.localScale = v;
                    //for (int i = 0; i < _transformGroup.Children.Count; i++)
                    //{
                    //    var child = _transformGroup.Children[i];
                    //    child.Target.localScale = child.ScaleOffset + v;
                    //}
                };
        };
        throw new System.NotImplementedException();
    }
}

public class TransformGroup
{   
    public Transform Parent { get; set; }
    public List<TransformChild> Children { get; private set; }
    
    public TransformGroup(Transform parent, List<Transform> group)
    {
        Parent = parent;
        Children = new List<TransformChild>();
        if(group == null || group.Count == 0)
            return;
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

    public void AddChild(Transform target)
    {
        var child = new TransformChild();
        child.Target = target;
        child.PositionOffset = target.localPosition - Parent.localPosition;
        child.RotationOffset = target.localRotation.eulerAngles - Parent.localRotation.eulerAngles;
        child.ScaleOffset = target.localScale - Parent.localScale;
        Children.Add(child);
    }

    public int RemoveChild(Transform target)
    {   
        var targetHash = target.GetHashCode();
        for (int i = 0; i < Children.Count; i++)
        {   
            var childHash = Children[i].Target.GetHashCode();
            if (childHash == targetHash)
            {
                Children.RemoveAtSwapBack(i);
                return i;
            }
        }
        return -1;
    }

    public struct TransformChild
    {   
        public Transform Target;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        public Vector3 ScaleOffset;
    }
}
