using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class SafeIMGUI
    {
        /// <summary>
        /// Executes an IMGUI draw action safely, catching and logging exceptions
        /// so the inspector window doesn't break.
        /// </summary>
        public static void Draw(Action drawAction, string context = null)
        {
            try
            {
                drawAction?.Invoke();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(context))
                    Debug.LogException(new Exception($"Error drawing IMGUI for {context}", ex));
                else
                    Debug.LogException(ex);

                // Optional: show a small inline help box so the inspector doesn't look empty
                EditorGUILayout.HelpBox(
                    $"Error drawing UI{(string.IsNullOrEmpty(context) ? "" : $" for {context}")}. See console for details.",
                    MessageType.Error
                );
            }
        }
    }
}