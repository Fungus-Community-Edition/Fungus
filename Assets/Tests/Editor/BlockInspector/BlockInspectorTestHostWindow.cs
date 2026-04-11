using AtMycelia.Hyphlow.EditorUtils;
using System;
using UnityEditor;
using UnityEngine;

namespace VScriptingTests.FCWindowOperations
{
    // Host window to provide a valid IMGUI Event context for OnInspectorGUI calls.
    public class BlockInspectorTestHostWindow : EditorWindow
    {
        public static BlockInspectorEditor EditorUnderTest;

        private void OnGUI()
        {
            if (EditorUnderTest != null)
            {
                try
                {
                    EditorUnderTest.OnInspectorGUI();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

}