using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita
{
    public class AmanitaRuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureHyphaTweenHierarchy()
        {
            if (Root != null)
            {
                return;
            }

            RootBootstrapper.EnsureRoot();
            var atMyceliaRoot = RootBootstrapper.Root;

            Root = CreateOurRoot();

            ValidateRoot(out bool success);
            if (!success)
            {
                return;
            }

            Root.transform.SetParent(atMyceliaRoot.transform);
        }

        public static GameObject Root { get; private set; }

        private static GameObject CreateOurRoot()
        {
            GameObject result = null;
            AmanitaManager manager;
            var prefab = Resources.Load<AmanitaManager>(OurRootPrefabPath);

            if (prefab == null)
            {
                string errorMessage = $"Could not find Amanita root prefab at " +
                    $"{OurRootPrefabPath}. The prefab is either missing or at " +
                    $"a different path.";
                Debug.LogError(errorMessage);
            }
            else
            {
                manager = UnityObj.Instantiate(prefab);
                manager.Init();
                result = manager.gameObject;
                result.name = prefab.name;
                UnityObj.DontDestroyOnLoad(result);
            }

            return result;
        }

        private const string OurRootPrefabPath = "Runtime/Prefabs/Amanita";

        private static void ValidateRoot(out bool success)
        {
            Root.TryGetComponent<AmanitaManager>(out var manager);
            success = manager != null;
            if (manager == null)
            {
                string errorMessage = $"Amanita root is missing its AmanitaManager component!";
                Debug.LogError(errorMessage);
            }
        }
    }
}