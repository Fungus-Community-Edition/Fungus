#if UNITY_EDITOR
using Amanita;
using Amanita.VScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Amanita.EditorUtils
{
    public static class GuidRegistryMaintenance
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [InitializeOnLoadMethod]
        public static void Init()
        {
            registries = FindAllGuidRegistriesInProject();
            flowchartGuidRegistries = registries.Where(reg => reg.StoresForType(flowchartFullName)).ToList();
            vsaGuidRegistries = registries.Where(reg => reg.StoresForType(vsaFullName)).ToList();

            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        private static IList<GuidRegistry> registries;

        private static IList<GuidRegistry> FindAllGuidRegistriesInProject()
        {
            List<GuidRegistry> registries = new List<GuidRegistry>();
            foreach (var guid in AssetDatabase.FindAssets("t:GuidRegistry"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var registry = AssetDatabase.LoadAssetAtPath<GuidRegistry>(path);
                if (registry != null)
                {
                    registries.Add(registry);
                }
            }
            return registries;
        }

        private static IList<GuidRegistry> flowchartGuidRegistries;
        private static IList<GuidRegistry> vsaGuidRegistries;

        private static void OnSceneSaved(Scene scene)
        {
            foreach (var registryEl in flowchartGuidRegistries)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    var flowchartsFound = root.GetComponentsInChildren<Flowchart>(true);
                    foreach (var fChart in flowchartsFound)
                    {
                        registryEl.RegisterUidOf(fChart);
                    }
                }

                registryEl.MarkDirtyAndSave();
            }

            AssetDatabase.Refresh();
        }

        private static readonly string flowchartFullName = typeof(Flowchart).FullName;
        private static readonly string vsaFullName = typeof(VariableSourceAsset).FullName;

    }
#endif
}