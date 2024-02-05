using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public enum TransformProperty
    {
        Position,
        Rotation,
        Scale
    }

    public static class EnumExtensions
    {
        public static Vector3 GetValue(this TransformProperty prop, Transform transform)
        {
            switch (prop)
            {
                case TransformProperty.Position:
                    return transform.position;
                case TransformProperty.Rotation:
                    return transform.eulerAngles;
                case TransformProperty.Scale:
                    return transform.localScale;
                default:
                    return Vector3.zero;
            }
        }
    }
}

