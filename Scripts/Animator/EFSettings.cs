using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public struct EFSettings
    {
        private const int c_preallocSize = 1024;
        private const int c_maxFuncBuffer = 8;

        public static int GroupAllocationSize { get { return c_preallocSize; } }
        public static int MaxFunctions { get {  return c_maxFuncBuffer; } }
    }
}

