using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow
{
    public static class HyphlowBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapOnRuntimeLoad()
        {
            EnsureHyphlowReady();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void EnsureHyphlowReady()
        {
            if (_essentialsHolder != null)
            {
                return;
            }

            var managerPrefab = Resources.Load<GameObject>(_pathToHyphlowManagerPrefab);
            _essentialsHolder = UnityObj.Instantiate(managerPrefab);
        }

        private static readonly string _pathToHyphlowManagerPrefab = "AtMycelia/Hyphlow/HyphlowManager";

        private static GameObject _essentialsHolder;

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureHyphlowReady();
        }
    }
}