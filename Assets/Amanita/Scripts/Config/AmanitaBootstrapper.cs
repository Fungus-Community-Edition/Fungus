using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtMycelia.Amanita
{
    public static class AmanitaBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapOnRuntimeLoad()
        {
            EnsureAmanitaReady();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAmanitaReady();
        }

        public static void EnsureAmanitaReady()
        {
            AmanitaManager manager = AmanitaManager.EnsureExists();
            if (manager != null)
            {
                manager.ApplySceneOverrides();
            }
        }
    }
}