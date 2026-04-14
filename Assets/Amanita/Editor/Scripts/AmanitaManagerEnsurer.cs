using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace AtMycelia.Amanita
{
    [InitializeOnLoad]
    public static class AmanitaManagerEnsurer 
    {
        static AmanitaManagerEnsurer()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            EnsureAmanitaManagerInScene();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EnsureAmanitaManagerInScene();
        }

        private static void EnsureAmanitaManagerInScene()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            AmanitaBootstrapper.EnsureAmanitaReady();
        }

        private static void OnAfterAssemblyReload()
        {
            EnsureAmanitaManagerInScene();
        }
    }
}