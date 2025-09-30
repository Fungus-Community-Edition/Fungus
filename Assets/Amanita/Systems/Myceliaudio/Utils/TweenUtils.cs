using UnityEngine;
using Type = System.Type;

namespace Amanita.Tweening
{
    public static class TweenUtils
    {
        public static bool EnsureValidTweener(ref ScriptableObject tweener, Type interfaceTypeNeeded,
            string whatItIsFor)
        {
            bool isItValid = ValidateTweener(tweener, interfaceTypeNeeded, whatItIsFor);
            if (!isItValid)
            {
                tweener = AmanitaManager.DefaultTweener;
            }
            return isItValid;
        }

        public static bool ValidateTweener(ScriptableObject tweener, Type interfaceTypeNeeded,
            string whatItIsFor)
        {
            bool implementsCorrectInterface = false;
            if (tweener == null)
            {
                Debug.LogWarning($"No tweener assigned. Switching to default linear tweener.");
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