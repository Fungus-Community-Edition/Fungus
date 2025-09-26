#if UNITY_EDITOR
using UnityEditor;
using System;

namespace Amanita.UI.EditorExt
{
    [CustomEditor(typeof(PlaytimeFormatter))]
    public class PlaytimeFormatterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var formatter = (PlaytimeFormatter)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("formatString"));

            // Test preview using a sample TimeSpan
            TimeSpan testSpan = new TimeSpan(1, 23, 45);
            string preview;
            try
            {
                // Need to escape backslashes, colons, and dots in the format string
                string safeFormat = formatter.FormatString
                    .Replace("\\", "\\\\")
                    .Replace(":", "\\:")
                    .Replace(".", "\\.");

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