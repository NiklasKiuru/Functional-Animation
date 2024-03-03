using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Aikom.FunctionalAnimation.Utility;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Data for animations
    /// </summary>
    public class TransformAnimation : ScriptableObject, ICustomIndexable<AnimationData, TransformProperty>
    {
        // **WARNING** Modifying this class can and will most likely wipe all existing animation data in the project
        [SerializeField] private float _duration;
        [SerializeField] private AnimationData[] _animationData = new AnimationData[3];

        public int Length => 3;
        public AnimationData this[TransformProperty index] { get => this[GetIndexer(index)]; }
        public float Duration { get => _duration; set => _duration = value; }
        public AnimationData this[int index] { get => _animationData[index]; }

#if UNITY_EDITOR
        public static TransformAnimation CreateNew(string savePath)
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

        public static TransformAnimation SaveAsNew(string savePath, TransformAnimation anim)
        {
            var asset = CreateInstance<TransformAnimation>();
            for(int i = 0; i < asset._animationData.Length; ++i)
            {   
                var data = new AnimationData();
                for(int j = 0; j < data.Length; ++j)
                {
                    data[j] = (GraphData)anim._animationData[i][j].Clone();
                }
                data.Mode = anim[i].Mode;
                data.Duration = anim[i].Duration;
                data.Start = anim[i].Start;
                data.Target = anim[i].Target;
                data.Offset = anim[i].Offset;
                data.Animate = anim[i].Animate;
                data.SeparateAxis = anim[i].SeparateAxis;
                data.TimeControl = anim[i].TimeControl;
                data.AnimateableAxis = anim[i].AnimateableAxis;

                asset._animationData[i] = data;
            }

            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

#endif
        /// <summary>
        /// Gets the selection matrix for the animation
        /// </summary>
        /// <returns></returns>
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

        public int GetIndexer(TransformProperty index)
        {
            switch (index)
            {
                case TransformProperty.Position:
                    return 0;
                case TransformProperty.Rotation:
                    return 1;
                case TransformProperty.Scale:
                    return 2;
                default:
                    throw new System.IndexOutOfRangeException();
            }
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

