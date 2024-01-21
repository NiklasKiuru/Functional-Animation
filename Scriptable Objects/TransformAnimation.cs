using UnityEngine;
using UnityEditor;

namespace Aikom.FunctionalAnimation
{
    public class TransformAnimation : ScriptableObject
    {
        [SerializeField] private TransformContainer<VectorContainer> _data;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _sync;
        [SerializeField] private float _duration;
        [SerializeField] private PropertyOptions _propertyOverrides;

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
    }

    public enum PropertyOptions
    {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
    }
}

