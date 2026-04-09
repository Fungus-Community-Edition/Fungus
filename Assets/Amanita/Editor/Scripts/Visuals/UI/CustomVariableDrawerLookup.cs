using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Fungus Variables are drawn via EditorGUI.Property by default, however, some types may require a custom replacement.
    /// The most common example is a Quaternion, Unity does not show you a quaternion as 4 floats, it shows you
    /// the euler angles, we also want to do that here
    ///
    /// This class is delegated to by editors to draw the actual variable property line.
    /// </summary>
    public static class CustomVariableDrawerLookup
    {
        //If you create new types that require custom singleline drawers, add them here
        public static Dictionary<System.Type, System.Action<UnityEngine.Rect, UnityEditor.SerializedProperty, GUIContent>> typeToDrawer =
            new Dictionary<System.Type, System.Action<Rect, UnityEditor.SerializedProperty, GUIContent>>()
            {
            };

        /// <summary>
        /// Called by editors that want a single line variable property drawn
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rect"></param>
        /// <param name="prop"></param>
        public static void DrawCustomOrPropertyField(System.Type type, Rect rect, SerializedProperty prop, GUIContent label)
        {
            System.Action<UnityEngine.Rect, UnityEditor.SerializedProperty, GUIContent> drawer = null;
            //delegate actual drawing to the variableInfo
            var foundDrawer = typeToDrawer.TryGetValue(type, out drawer);
            if (foundDrawer)
            {
                drawer(rect, prop, label);
            }
            else
            {
                EditorGUI.PropertyField(rect, prop, label);
            }
        }
    }
}