using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Custom drawer for the AnyVaraibleAndDataPair, shows only the matching data for the targeted variable
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnyVariableAndDataPair))]
    public class AnyVariableAndDataPairDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty holdsVarAndDataPair, GUIContent label)
        {
            SerializedProperty leftHandSideVarProp;
            DisplayLeftHandSideVar();
            void DisplayLeftHandSideVar()
            {
                leftHandSideVarProp = holdsVarAndDataPair.FindPropertyRelative("variable");
                EditorGUI.PropertyField(position, leftHandSideVarProp, label);
            }

            position.y += EditorGUIUtility.singleLineHeight;

            HandleInnerDataField();
            void HandleInnerDataField()
            {
                IVariable currentLeftHandSideVar = leftHandSideVarProp.objectReferenceValue as IVariable;
                HandleLhsVarChanges();
                void HandleLhsVarChanges()
                {
                    SerializedProperty anyVarDataProp = holdsVarAndDataPair.FindPropertyRelative("data");
                    // ^Which can be a literal val or a var
                    AnyVariableData anyVarData = anyVarDataProp.managedReferenceValue as AnyVariableData;
                    

                    bool lhsVarChanged = !ReferenceEquals(_prevLeftHandSideVar, currentLeftHandSideVar);
                    bool validAnyVarData = anyVarData != null; 
                    if (lhsVarChanged && validAnyVarData && currentLeftHandSideVar != null)
                    {
                        // When currentLeftHandSideVar is null, we don't want to change the var type
                        // of the inner data field. Later in this func, we'll just make sure
                        // not to render it
                        Debug.Log($"Updating the var type of the rhs");
                        anyVarData.SetFor(currentLeftHandSideVar.GetType(), currentLeftHandSideVar.ContentType);
                        _prevLeftHandSideVar = currentLeftHandSideVar;
                        holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();
                    }
                }

                DrawInnerDataField();
                void DrawInnerDataField()
                {
                    SerializedProperty innerDataProp = holdsVarAndDataPair.FindPropertyRelative("data.data");
                    // ^Expected to hold a VariableData subclass as its boxed and object ref values

                    if (currentLeftHandSideVar != null && innerDataProp != null)
                    {
                        // Let Unity's property drawer system handle drawing the data
                        EditorGUI.PropertyField(position, innerDataProp, new GUIContent("Data"));
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Must select a variable before setting data.");
                    }
                }
            }

            GUILayout.Space(20);
            holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();
        }

        protected IVariable _prevLeftHandSideVar;

        protected static bool TryGetTypeActionsFor(System.Type varPropType, out VariableTypeActions typeActionsRes)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varPropType, out typeActionsRes);
        }

    }
}