using AtMycelia.Amanita.EditorUtils;
using AtMycelia.Amanita.VScripting;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita
{
    [InitializeOnLoad]
    public static class AmanitaManagerEnsurer 
    {
        static AmanitaManagerEnsurer()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EnsureAmanitaManagerInScene();
        }

        private static void EnsureAmanitaManagerInScene()
        {
            // But only if there's at least one Flowchart in it already.
            Flowchart fc = UnityObj.FindFirstObjectByType<Flowchart>();
            if (fc != null && AmanitaManager.S == null)
            {
                AmanitaManager.EnsureExists();
            }
        }

        private static void OnAfterAssemblyReload()
        {
            EnsureAmanitaManagerInScene();
        }
    }
}