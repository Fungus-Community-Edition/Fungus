#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using Amanita.SaveSys.UI;

namespace Amanita.SaveSys.UI.EditorExt
{
    [CustomEditor(typeof(PlaytimeFormatter))]
    public class PlaytimeFormatterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var formatter = (PlaytimeFormatter)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("inTextForm"));

            // Test preview using a sample TimeSpan
            TimeSpan testSpan = new TimeSpan(1, 23, 45);
            string preview;
            try
            {
                string safeFormat = formatter.InTextForm
                    .Replace("\\", "\\\\")  // escape backslashes
                    .Replace(":", "\\:");    // escape colons

                preview = testSpan.ToString(safeFormat);
            }
            catch (FormatException)
            {
                preview = "<Invalid Format>";
            }

            EditorGUILayout.LabelField("Preview", preview, EditorStyles.helpBox);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}