using AtMycelia.Hyphlow.Sys;
using UnityEngine;
using Type = System.Type;

namespace AtMycelia.AmaniTween
{
    public static class TweenUtils
    {
        public static bool EnsureValidTweener(ref ScriptableObject tweener, Type interfaceTypeNeeded,
            string whatItIsFor, bool logMessages = true)
        {
            bool isItValid = ValidateTweener(tweener, interfaceTypeNeeded, whatItIsFor, logMessages);
            if (!isItValid)
            {
                tweener = HyphlowRuntimeSysAssets.S.TweenAdapter;
            }
            return isItValid;
        }

        public static bool ValidateTweener(ScriptableObject tweener, Type interfaceTypeNeeded,
            string whatItIsFor, bool logMessages = true)
        {
            bool implementsCorrectInterface = false;
            if (tweener == null)
            {
                if (logMessages)
                {
                    Debug.LogWarning($"No tweener assigned. Switching to default linear tweener.");
                }
            }
            else
            {
                implementsCorrectInterface = interfaceTypeNeeded.IsAssignableFrom(tweener.GetType());
                if (!implementsCorrectInterface)
                {
                    Debug.LogWarning($"Tweener {tweener.name} is invalid for {whatItIsFor}. Needs to implement " +
                        $"{interfaceTypeNeeded.Name}. Switching to default linear tweener.");
                }
            }

            bool isItValid = tweener != null && implementsCorrectInterface;
            return isItValid;
        }
    }
}