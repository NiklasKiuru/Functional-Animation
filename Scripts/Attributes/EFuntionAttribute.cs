using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EFuntionAttribute : PropertyAttribute
    {
        public bool UseImplicitAmplitudeModulation;

        public EFuntionAttribute(bool useImplicitAmplitudeModulation = true)
        {
            UseImplicitAmplitudeModulation = useImplicitAmplitudeModulation;
        }
    }
}

