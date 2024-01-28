using Aikom.FunctionalAnimation.Extensions;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class RuntimeController
    {
        private MatrixRxC<Interpolator<float>> _axisInterpolators;
        private Interpolator<Vector3>[] _vectorInterpolators = new Interpolator<Vector3>[3] 
        { 
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero), 
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero),
            new Interpolator<Vector3>(null, null, 0, Vector3.zero, Vector3.zero)
        };
        private MatrixRxC<bool> _animationChecks;
        private Transform _target;
        private TimeKeeper _mainClock;

        internal Interpolator<Vector3>[] VectorInterpolators { get => _vectorInterpolators; }

        public RuntimeController()
        {
            _mainClock = new TimeKeeper(0);
        }

        internal void SetAnimation(TransformAnimation anim, Transform target)
        {
            _animationChecks = anim.GetSelectionMatrix();
            _target = target;
            _axisInterpolators = new MatrixRxC<Interpolator<float>>(3, 3);
            _vectorInterpolators = new Interpolator<Vector3>[3];
            var properties = (TransformProperty[])System.Enum.GetValues(typeof(TransformProperty));
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

                _vectorInterpolators[i] = new Interpolator<Vector3>(incrimentFunc, setValFunc, 
                    1 / data.Duration, GetPropValue(prop), GetPropValue(prop) + data.Offset, data.TimeControl);
            }
        }

        public void Update()
        {
            for(int i = 0; i < _vectorInterpolators.Length; i++)
            {
                _vectorInterpolators[i]?.Run();
            }
        }

        private Func<float, Vector3, Vector3, Vector3> IncrimentAll(Func<float, float> ease)
        {
            return (t, start, end) => UnityExtensions.Interpolate(ease, start, end, t);
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
                        var vector = new Vector3(animateableAxis.x ? v.x : _target.localPosition.x, animateableAxis.y ? v.y : _target.localPosition.y, animateableAxis.z ? v.z : _target.localPosition.z);
                        _target.localPosition = vector;
                    };
                case TransformProperty.Rotation:
                    return (v) =>
                    {
                        var vector = new Vector3(animateableAxis.x ? v.x : _target.localRotation.eulerAngles.x, animateableAxis.y ? v.y : _target.localRotation.eulerAngles.y, animateableAxis.z ? v.z : _target.localRotation.eulerAngles.z);
                        _target.localRotation = Quaternion.Euler(vector);
                    };
                case TransformProperty.Scale:
                    return (v) =>
                    {
                        var vector = new Vector3(animateableAxis.x ? v.x : _target.localScale.x, animateableAxis.y ? v.y : _target.localScale.y, animateableAxis.z ? v.z : _target.localScale.z);
                        _target.localScale = vector;
                    };
            }
            throw new System.NotImplementedException();
        }

        private Action<Vector3> DefineSetValueFunctionAll(TransformProperty prop)
        {
            return prop switch
            {
                TransformProperty.Position => (v) => _target.localPosition = v,
                TransformProperty.Rotation => (v) => _target.localRotation = Quaternion.Euler(v),
                TransformProperty.Scale => (v) => _target.localScale = v,
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}

