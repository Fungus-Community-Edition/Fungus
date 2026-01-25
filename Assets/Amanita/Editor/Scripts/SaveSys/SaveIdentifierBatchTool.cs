using UnityEditor;
using UnityEngine;

namespace Amanita.SaveSys.CustEditor
{
    public class SaveIdentifierBatchTool : EditorWindow
    {
        [MenuItem("Tools/Regenerate Save IDs")]
        public static void ShowWindow()
        {
            GetWindow<SaveIdentifierBatchTool>("Regenerate Save IDs");
        }

        private void OnGUI()
        {
            GUILayout.Label("Regenerate Unique IDs for Selected Objects", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate New IDs"))
            {
                foreach (var obj in Selection.gameObjects)
                {
                    var identifier = obj.GetComponent<SaveIdentifier>();
                    if (identifier != null)
                    {
                        Undo.RecordObject(identifier, "Generate Unique ID");
                        identifier.GetSelfNewID();
                        EditorUtility.SetDirty(identifier);
                    }
                }
            }
        }
    }
}