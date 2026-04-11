using UnityEngine;

namespace AtMycelia
{
    public static class MonoBehaviourExtensions
    {
        public static void SafeStopCoroutine(this MonoBehaviour monoBehaviour, Coroutine coroutine)
        {
            if (coroutine != null)
            {
                monoBehaviour.StopCoroutine(coroutine);
            }
        }
    }
}