using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class EFunctionAttribute : PropertyAttribute
    {
        public bool UseImplicitAmplitudeModulation { get; private set; }
        public string Name { get; private set; }

        public EFunctionAttribute(string name = "", bool useImplicitAmplitudeModulation = true)
        {
            UseImplicitAmplitudeModulation = useImplicitAmplitudeModulation;
            Name = name;
        }
    }
}

