using UnityEngine;

namespace AtMycelia.Amanita
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