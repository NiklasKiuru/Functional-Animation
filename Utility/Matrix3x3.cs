using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public struct Matrix3x3
    {
        public float m00;
        public float m01;
        public float m02;
        public float m10;
        public float m11;
        public float m12;
        public float m20;
        public float m21;
        public float m22;
        
        public Matrix3x3(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            m20 = position.x;
            m21 = position.y;
            m22 = position.z;
            m10 = rotation.x;
            m11 = rotation.y;
            m12 = rotation.z;
            m00 = scale.x;
            m01 = scale.y;
            m02 = scale.z;
        }
    }
}

