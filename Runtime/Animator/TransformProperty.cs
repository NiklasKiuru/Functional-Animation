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

        public static void SetValue(this TransformProperty prop, Transform tr, Vector3 val)
        {
            switch (prop)
            {
                case TransformProperty.Position:
                    tr.position = val;
                    break;
                case TransformProperty.Rotation:
                    tr.rotation = Quaternion.Euler(val); 
                    break;
                case TransformProperty.Scale:
                    tr.localScale = val;
                    break;
            }
        }
    }
}

