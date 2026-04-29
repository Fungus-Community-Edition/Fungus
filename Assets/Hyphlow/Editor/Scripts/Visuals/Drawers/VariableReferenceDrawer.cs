using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Custom drawer for VariableReference, allows selecting a target variable.
    /// Supports filtering via ContentTypeConstraint.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public VariableReferenceDrawer()
        {
            
            if (!Application.isPlaying)
            {
                GameObject activeGo = Selection.activeGameObject;
                Flowchart fc = null;
                if (activeGo != null && activeGo.TryGetComponent(out Flowchart found))
                {
                    fc = found;
                }
                else
                {
                    // Fallback to whatever the FlowchartWindow is working with.
                    FlowchartWindow fcWindow = FlowchartWindow.S;
                    if (fcWindow != null)
                    {
                        fc = fcWindow.Flowchart;
                    }
                }
                VariableRegistryService.RebuildAll(fc);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty varRefProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varRefProp);

            varRefProp.serializedObject.Update();

            UnityObj targetObject = varRefProp.serializedObject.targetObject;
            Type[] allowedContentTypes = GetAllowedTypes(fieldInfo);
            VariableRegistry varRegistry = null;

            EnsurePrerequisites(out bool canContinue);
            void EnsurePrerequisites(out bool success)
            {
                success = false;

                varRegistry = VariableRegistryService.Registry;
                if (varRegistry == null)
                {
                    EditorGUI.LabelField(position, label.text, "Variable registry not available.");
                    EditorGUI.EndProperty();
                    return;
                }

                success = true;
            }

            if (!canContinue)
            {
                return;
            }

            var validVarsInScene = varRegistry.GetVarsOfMultiTypes(allowedContentTypes);

            List<IVariable> candidates = validVarsInScene.Values.ToList();
            string[] options = validVarsInScene.Keys
                .Prepend("<None>")
                .ToArray();

            SerializedProperty itemIdProp = varRefProp.FindPropertyRelative("_itemId");
            SerializedProperty owningSourceProp = varRefProp.FindPropertyRelative("_owningSource");

            int currentItemId = itemIdProp.intValue;
            UnityObj storedOwner = owningSourceProp.objectReferenceValue;

            int currentIndex = 0;
            bool validId = currentItemId != Muscariable.InvalidId;
            if (validId)
            {
                int found = candidates.FindIndex(IsVarWithRightIdAndOwner);

                if (found >= 0)
                {
                    currentIndex = found + 1; // +1 because of the <None> option at index 0
                }
            }

            bool IsVarWithRightIdAndOwner(IVariable varEl)
            {
                // To avoid ID collision issues, we also check that the owner of the variable
                // matches the stored owner reference. This way, even if there are multiple
                // variables with the same ID, we should still show the correct one as
                // selected in the dropdown.
                bool rightId = varEl != null && varEl.ItemId == currentItemId;
                if (!rightId)
                {
                    return false;
                }

                if (storedOwner == null)
                {
                    return true;
                }
                bool rightOwner = ReferenceEquals(varEl.Owner as UnityObj, storedOwner);
                return rightId && rightOwner;
            }
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
            // ^This is what lets the user choose a variable from the dropdown, and it returns the index of the chosen option

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetObject, "Set Variable Reference");

                bool choseToSetNullVar = newIndex == 0;
                if (choseToSetNullVar)
                {
                    itemIdProp.intValue = Muscariable.InvalidId;
                    owningSourceProp.objectReferenceValue = null;
                }
                else
                {
                    IVariable chosen = candidates[newIndex - 1];
                    // ^Need the -1 because of the <None> option at index 0
                    itemIdProp.intValue = chosen.ItemId;
                    owningSourceProp.objectReferenceValue = chosen.Owner as UnityObj;
                }

                varRefProp.serializedObject.ApplyModifiedProperties();

                if (PrefabUtility.IsPartOfPrefabInstance(targetObject))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
                }

                EditorUtility.SetDirty(targetObject);
                GameObject go = null;
                if (targetObject is GameObject)
                {
                    go = targetObject as GameObject;
                }
                else if (targetObject is Component)
                {
                    go = (targetObject as Component).gameObject;
                }
                PrefabStage prefabStage = PrefabStageUtility.GetPrefabStage(go);
                if (prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static Type[] GetAllowedTypes(FieldInfo fieldInfo)
        {
            Type[] result;
            var attr = fieldInfo.GetCustomAttribute<ContentTypeConstraintAttribute>();
            if (attr != null && attr.AllowedTypes != null && attr.AllowedTypes.Count > 0)
            {
                result = attr.AllowedTypes.ToArray();
            }
            else
            {
                result = Array.Empty<Type>();
            }
            return result;
        }
    }
}