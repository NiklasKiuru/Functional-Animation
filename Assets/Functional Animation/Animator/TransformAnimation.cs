using UnityEngine;
using UnityEditor;
using Aikom.FunctionalAnimation.Utility;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Data for animations
    /// </summary>
    public class TransformAnimation : ScriptableObject, IIndexable<AnimationData, TransformProperty>
    {
        // **WARNING** Modifying this class can and will most likely wipe all existing animation data in the project
        [SerializeField] private float _duration;
        [SerializeField] private AnimationData[] _animationData = new AnimationData[3];

        public AnimationData[] AnimationData => _animationData;
        public int Length => 3;
        public AnimationData this[TransformProperty index] { get => _animationData[(int)index]; }
        public float Duration { get => _duration; internal set => _duration = value; }

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
    }

    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2,
        W = 3,
    }
}

