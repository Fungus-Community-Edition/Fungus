using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomPropertyDrawer(typeof(AnyVariableData), true)]
    public class AnyVariableDataDrawer : VariableDataDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            AnyVariableData varData = varDataProp.boxedValue as AnyVariableData;
            ContentTypeConstraintAttribute constraintAttr = fieldInfo != null
                ? fieldInfo.GetCustomAttribute<ContentTypeConstraintAttribute>()
                : null;

            if (varData != null && varData.ContentType == null && constraintAttr != null &&
                constraintAttr.AllowedTypes != null && constraintAttr.AllowedTypes.Count > 0)
            {
                varData.SetFor(constraintAttr.AllowedTypes[0]);
                varDataProp.boxedValue = varData;
            }

            base.OnGUI(position, varDataProp, label);
        }
    }
}