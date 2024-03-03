using System.IO.Enumeration;
using Unity.Mathematics;
using System;

namespace Aikom.FunctionalAnimation
{
    public class TransformHandle
    {
        public IInterpolatorHandle<float3> Position { get; private set; }
        public IInterpolatorHandle<float3> Rotation { get; private set; }
        public IInterpolatorHandle<float3> Scale { get; private set; }

        public bool IsActive { get => math.any(new bool3(
            TryGetAliveHandle(TransformProperty.Position,out _), 
            TryGetAliveHandle(TransformProperty.Rotation, out _), 
            TryGetAliveHandle(TransformProperty.Scale, out _))); }

        public TransformHandle()
        {
        }

        public bool TryGetHandle(TransformProperty prop, out IInterpolatorHandle<float3> handle)
        {
            handle = null;
            if(prop == TransformProperty.Position && Position != null)
            {
                handle = Position;
                return true;
            }
            if (prop == TransformProperty.Rotation && Rotation != null)
            {
                handle = Rotation;
                return true;
            }
            if (prop == TransformProperty.Scale && Scale != null)
            {
                handle = Scale;
                return true;
            }
            return false;
        }

        public bool TryGetAliveHandle(TransformProperty prop, out IInterpolatorHandle<float3> handle)
        {
            if(TryGetHandle(prop, out handle) && handle.IsAlive)
                return true;
            handle = null;
            return false;
        }

        internal void Set(TransformProperty prop, IInterpolatorHandle<float3> handle)
        {
            switch (prop)
            {
                case TransformProperty.Position:
                    Position = handle;
                    break;
                case TransformProperty.Rotation:
                    Rotation = handle;
                break;
                    case TransformProperty.Scale:
                    Scale = handle;
                break;
            }
        }

        public void KillAll()
        {
            var props = (TransformProperty[])Enum.GetValues(typeof(TransformProperty));
            foreach (var prop in props)
            {
                if(TryGetAliveHandle(prop, out var handle))
                    handle.Kill();
            }
        }
    }
}

