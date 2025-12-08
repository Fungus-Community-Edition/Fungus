using UnityEngine;
using UnityEditor;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartMenuItems
    {
        [MenuItem("Tools/Amanita/Create/Flowchart", false, 0)]
        static void CreateFlowchart()
        {
            GameObject go = SpawnPrefab("Flowchart");
            go.transform.position = Vector3.zero;

            // This is the latest version of Flowchart, so no need to update.
            var flowchart = go.GetComponent<Flowchart>();
            if (flowchart != null)
            {
                flowchart.Version = AmanitaConstants.CurrentVersion;
            }

            // Only the first created Flowchart in the scene should have a default GameStarted block
            var flowchartsFound = GameObject.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            if (flowchartsFound.Length > 1)
            {
                var block = go.GetComponent<Block>();
                GameObject.DestroyImmediate(block._EventHandler);
                block._EventHandler = null;
            }
        }

        [MenuItem("Tools/Amanita/Create/Fungus Logo", false, 1000)]
        static void CreateFungusLogo()
        {
            SpawnPrefab("Fungus Logo");
        }

        public static GameObject SpawnPrefab(string prefabName)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
            if (prefab == null)
            {
                return null;
            }

            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            go.name = prefab.name;

            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                Camera sceneCam = view.camera;
                Vector3 pos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                pos.z = 0f;
                go.transform.position = pos;
            }

            Selection.activeGameObject = go;
            
            Undo.RegisterCreatedObjectUndo(go, "Create Object");

            return go;
        }
    }
}