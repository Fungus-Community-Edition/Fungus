using AtMycelia.Hyphlow.EditorUtils;
using UnityEditor;
using UnityEngine;

namespace VScriptingTests.VariableOperations
{
    public class VariableReferenceDrawerHostWindow : EditorWindow
    {
        internal VariableReferenceDrawer Drawer;
        internal SerializedObject SO;
        internal SerializedProperty VarRefProp;          // The struct property (VariableReference)
        internal SerializedProperty InnerVariableProp;   // The 'variable' child inside the struct
        internal GUIContent Label = new GUIContent("Variable Reference");
        internal bool DidDraw;

        private void OnGUI()
        {
            if (Drawer == null || SO == null || VarRefProp == null)
                return;

            var rect = new Rect(4, 4, position.width - 8, Drawer.GetPropertyHeight(VarRefProp, Label));
            Drawer.OnGUI(rect, VarRefProp, Label);
            DidDraw = true;
        }
    }
}