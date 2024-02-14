using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public interface ITransformHandle : IInterpolator<float3x3>
    {
        public IInterpolatorHandle<float3> Position { get; }
        public IInterpolatorHandle<float3> Rotation { get; }
        public IInterpolatorHandle<float3> Scale { get; }
    }
}

