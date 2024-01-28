using UnityEngine;
using UnityEditor;
using System;
using Unity.Mathematics;
using Aikom.FunctionalAnimation.Utility;

namespace Aikom.FunctionalAnimation
{
    public class TransformAnimation : ScriptableObject, IIndexable<AnimationData, TransformProperty>
    {
        private TransformContainer<VectorContainer> _data;
        private bool _loop;
        private bool _sync;
        private float _duration;
        private PropertyOptions _propertyOverrides;

        public TransformContainer<VectorContainer> Data => _data;
        public bool Loop => _loop;
        public bool Sync => _sync;
        public float Duration => _duration;
        public PropertyOptions PropertyOverrides => _propertyOverrides;

        internal static TransformAnimation Save(TransformAnimator animator, string savePath, PropertyOptions propOverrides)
        {
            var asset = CreateInstance<TransformAnimation>();
            asset._data = new TransformContainer<VectorContainer>();
            asset._data.Position = animator.Container.Position.Clone() as VectorContainer;
            asset._data.Rotation = animator.Container.Rotation.Clone() as VectorContainer;
            asset._data.Scale = animator.Container.Scale.Clone() as VectorContainer;
            asset._propertyOverrides = propOverrides;
            asset._loop = animator.Loop;
            asset._sync = animator.SyncAll;
            asset._duration = animator.MaxDuration;
            
            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            
            return asset;
        }

        [SerializeField] private AnimationData[] _animationData = new AnimationData[3];

        public AnimationData[] AnimationData => _animationData;

        public int Length => 3;

        public AnimationData this[TransformProperty index] { get => _animationData[(int)index]; set => _animationData[(int)index] = value; }

        public void Load(ref GraphData[] target, TransformProperty prop, Axis axis)
        {
            //var targetProp = _animationData[(int)prop];
            //var targetAxis = targetProp[(int)axis];
            //target = new FunctionData[targetAxis.Length];
            //for (int i = 0; i < targetAxis.Length; i++)
            //{
            //    target[i] = targetAxis[i];
            //}
        }

        public static TransformAnimation SaveNew(string savePath)
        {
            var asset = CreateInstance<TransformAnimation>();
            for(int i = 0; i < asset._animationData.Length; i++)
            {
                asset._animationData[i] = new AnimationData();
            }
            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        public MatrixRxC<bool> GetSelectionMatrix()
        {
            var mat = new MatrixRxC<bool>(3, 4);
            for(int i = 0; i < Length; i++)
            {   
                var data = _animationData[i];
                if (!data.SeparateAxis)
                {
                    mat.SetRow(i, new bool[] { false, false, false, true });
                    continue;
                }
                else
                {   
                    var row = new bool[4] { data.AnimateableAxis[0], data.AnimateableAxis[1], data.AnimateableAxis[2], false };
                    mat.SetRow(i, row);
                }
            }
            return mat;
        }

        public AnimationData[] GetData() => _animationData;
    }

    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2,
        W = 3,
    }

    

    public enum PropertyOptions
    {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }
}

