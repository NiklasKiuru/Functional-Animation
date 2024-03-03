using Aikom.FunctionalAnimation;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class GroupController : IDisposable, IAnimator
{
    private MatrixRxC<bool> _animationChecks;
    private bool3x4 _jobAxisChecks;
    private Transform _target;
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
    private TransformHandle _handle;
    private bool3 _propChecks;

    public bool IsActive 
    { 
        get 
        { 
            if (_handle == null)
                return false;
            else
                return _handle.IsActive;    
        } 
    }

    TransformHandle IAnimator.Handle => _handle;

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
    internal void Start(TransformAnimation anim, Transform target, List<Transform> group)
    {   
        TransformAccessArray.Allocate(group.Count, _threadCount, out _transformAccessArray);
        for(int i = 0; i < group.Count; i++)
        {
            _transformAccessArray.Add(group[i]);
        }
        _offsets = new NativeList<float3x3>(group.Count, Allocator.Persistent);
        _currentValues = new float3x3();
        _originValues = new float3x3();
        _originValues.c0 = new float3(target.position.x, target.localEulerAngles.x, target.localScale.x);
        _originValues.c1 = new float3(target.position.y, target.localEulerAngles.y, target.localScale.y);
        _originValues.c2 = new float3(target.position.z, target.localEulerAngles.z, target.localScale.z);
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
        _data = anim;
        _target = target;

        _handle = new TransformHandle();
        _propChecks = new bool3(anim[0].Animate, anim[1].Animate, anim[2].Animate);
        this.SetAnimation(anim, target, SetVal);
        void SetVal(float3 val, TransformProperty prop)
        {
            prop.SetValue(target, val);
            _currentValues[(int)prop] = val;
        }
    }

    /// <summary>
    /// Completes the current job if it is running, disposes all unmanaged containers and marks the controller as inactive
    /// </summary>
    public void Disable()
    {
        if(!_jobHandle.IsCompleted)
            _jobHandle.Complete();
        Dispose();
    }

    /// <summary>
    /// Adds a new child to the current group
    /// </summary>
    /// <param name="target"></param>
    public void AddGroupChild(Transform target)
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
    public void Update()
    {
        if (!IsActive)
            return;
        _job = new SyncTransformGroupJob()
        {
            Offsets = _offsets,
            CurrentValues = _currentValues,
            OriginValues = _originValues,
            AxisCheck = _jobAxisChecks,
            PropCheck = _propChecks,
        };

        _jobHandle = _job.Schedule(_transformAccessArray);
    }

    /// <summary>
    /// Completes the current jobhandle. Must be called in late update
    /// </summary>
    public void CompleteJobs()
    {
        _jobHandle.Complete();
        CheckInactiveGroupChildren();
    }

    /// <summary>
    /// Disposes unmanaged memeory and destroys the virtual parent
    /// </summary>
    public void Dispose()
    {
        _handle.KillAll();
        _handle = null;
        if (_offsets.IsCreated) _offsets.Dispose();
        if(_transformAccessArray.isCreated) _transformAccessArray.Dispose();
        if(_target != null)
            UnityEngine.Object.Destroy(_target.gameObject);
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
