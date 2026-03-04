using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.SaveSys
{
    public static class SaveSystemBootstrapper
    {
        private const string BootstrapObjectName = "MarasmiusSaveSystem";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (SaveSystem.S != null && SaveSystemInstaller.S != null)
            {
                return;
            }

            SaveSystem saveSystem = UnityObj.FindFirstObjectByType<SaveSystem>(FindObjectsInactive.Include);
            SaveSystemInstaller installer = UnityObj.FindFirstObjectByType<SaveSystemInstaller>(FindObjectsInactive.Include);

            GameObject root = null;
            if (saveSystem != null)
            {
                root = saveSystem.gameObject;
            }
            else if (installer != null)
            {
                root = installer.gameObject;
            }

            if (root == null)
            {
                root = new GameObject(BootstrapObjectName);
                UnityObj.DontDestroyOnLoad(root);
            }
            else
            {
                UnityObj.DontDestroyOnLoad(root);
            }

            if (saveSystem == null)
            {
                saveSystem = root.AddComponent<SaveSystem>();
            }

            if (installer == null)
            {
                installer = root.AddComponent<SaveSystemInstaller>();
            }

            installer.Init();
        }
    }
}