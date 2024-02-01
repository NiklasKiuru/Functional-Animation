using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aikom.FunctionalAnimation.Utility;

namespace Aikom.FunctionalAnimation
{
    public class Animator : MonoBehaviour
    {
        public static Animator Instance => (ApplicationUtils.IsQuitting || _instance != null) ? _instance : (_instance = CreateInstance());
        protected static Animator _instance;
        

        private static Animator CreateInstance()
        {
            var gameObject = new GameObject(nameof(Animator))
            {
                hideFlags = HideFlags.DontSave,
            };
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            else
#endif
            {
                DontDestroyOnLoad(gameObject);
            }
            return gameObject.AddComponent<Animator>();
        }

        private void RunClocks()
        {

        }

        private void Update()
        {
            RunClocks();
        }

    }
}

