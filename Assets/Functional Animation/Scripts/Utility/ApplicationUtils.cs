using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation.Utility
{
    public static class ApplicationUtils
    {
        public static bool IsQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeMethod()
        {
            IsQuitting = false;
            Application.quitting += OnQuitting;
        }

        private static void OnQuitting()
        {
            IsQuitting = true;
            Application.quitting -= OnQuitting;
        }
    }
}

