#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    // Separate file (already referenced in project open docs) – ensure it still matches usage.
    public class CommandEditorTestHostWindow : EditorWindow
    {
        public CommandEditor EditorUnderTest;

        private void OnGUI()
        {
            if (EditorUnderTest == null) return;
            try
            {
                var mi = typeof(CommandEditor).GetMethod("DrawCommandInspectorGUI",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                mi?.Invoke(EditorUnderTest, null);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
#endif