using UnityEngine;
using UnityEditor;

namespace Amanita.SaveSys.CustEditor
{
    [CustomEditor(typeof(SaveIdentifier))]
    public class SaveIdentifierEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SaveIdentifier saveIdentifier = (SaveIdentifier)target;

            if (GUILayout.Button("Generate New Unique ID"))
            {
                Undo.RecordObject(saveIdentifier, "Generate Unique ID");
                saveIdentifier.GetSelfNewID();
                EditorUtility.SetDirty(saveIdentifier);
            }
        }
    }
}