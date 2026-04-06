using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace AtMycelia.EditorUtils
{
    /// <summary>
    /// Ensures that in response to certain editor events, the ApplyBackwardsCompatibility of all
    /// Components implementing IBackwardsCompatibilityApplier is called.
    /// </summary>
    [InitializeOnLoad]
    public static class BackwardsCompatibilityEnsurer 
    {
        static BackwardsCompatibilityEnsurer()
        {
            ToggleSubs(false);
            ToggleSubs(true);
        }

        static void ToggleSubs(bool on)
        {
            ToggleSubsEditorOnly(on);

            if (on)
            {
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
            }
            else
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            }
        }

        private static void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            ApplyBackwardsCompatibilityToAllAppliers();
        }

        static void ToggleSubsEditorOnly(bool on)
        {
#if UNITY_EDITOR
            if (on)
            {
                AssemblyReloadEvents.afterAssemblyReload += ApplyBackwardsCompatibilityToAllAppliers;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorApplication.delayCall += ApplyBackwardsCompatibilityToAllAppliers;
            }
            else
            {
                AssemblyReloadEvents.afterAssemblyReload -= ApplyBackwardsCompatibilityToAllAppliers;
                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorApplication.delayCall -= ApplyBackwardsCompatibilityToAllAppliers;
            }
#endif
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ApplyBackwardsCompatibilityToAllAppliers();
        }

        private static void ApplyBackwardsCompatibilityToAllAppliers()
        {
            var appliers = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var applier in appliers)
            {
                if (applier is IBackwardsCompatibilityApplier backwardsCompatibilityApplier)
                {
                    backwardsCompatibilityApplier.ApplyBackwardsCompatibility();
                }
            }
        }
    }
}