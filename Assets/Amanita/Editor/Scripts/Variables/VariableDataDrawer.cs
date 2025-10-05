using Amanita.EditorUtils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    // For the fields that can accept either a variable or a literal value
    [CustomPropertyDrawer(typeof(VariableData), true)]
    public class VariableDataDrawer<T> : PropertyDrawer
    {
        protected readonly DefaultEditorAssetResolver _assetResolver = new DefaultEditorAssetResolver();

        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varDataProp);
            VariableData varData = varDataProp.boxedValue as VariableData;
            varData.Refresh();
            varDataProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Find the two key sub-properties
            SerializedProperty valueProp, referenceProp;
            try
            {
                valueProp = varDataProp.FindPropertyRelative("valOfType");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception trying to find 'valOfType' property relative to {varDataProp.propertyPath}. " +
                    $"Its display name: {varDataProp.displayName}. Make sure the VariableData class still has a field named 'valOfType'. Exception: {e}");
                throw;
            }
            referenceProp = varDataProp.FindPropertyRelative("varRef");

            // Layout: label, then value/reference side-by-side
            int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
            const int popupGap = 5;
            Rect wholeFieldRect = EditorGUI.PrefixLabel(position, label);
            Rect valueRect = wholeFieldRect;
            int spaceForPopup = popupWidth + popupGap;
            valueRect.width = Mathf.Max(0, wholeFieldRect.width - spaceForPopup);
            // ^We want to make sure that the rect for the value field leaves enough space for the popup
            Rect popupRect = wholeFieldRect; 
            popupRect.x += valueRect.width + popupGap;
            popupRect.width = popupWidth;

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // We only want to draw the literal value when the varRef is null
            bool shouldDrawLiteral = !VarRefPropHasAnythingAssigned(referenceProp);
            if (shouldDrawLiteral)
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            // Draw the variable reference (branch on propertyType)
            // Going to need to define some new logic here, since the old stuff was predicated
            // on the var refs having VariableInfo attributes, which they no longer do.

            DrawReferenceField();
            void DrawReferenceField()
            {
                Flowchart localFlowchart = FlowchartWindow.GetFlowchart();
                if (localFlowchart == null)
                {
                    Debug.LogWarning($"No flowchart is open in the Flowchart window. Cannot draw variable reference field for {varDataProp.propertyPath}.");
                    return;
                }

                int index = 0, selectedIndex = 0;
                // ^So we can track which var in the dropdown is currently selected
                // NOTE: previously this cast assumed the managed reference would be a VariablePointer.
                // Muscariable instances are pure CLR objects (not UnityObj-backed pointers) so
                // we must inspect the boxed value as the general IVariable instead of only IVariablePointer.
                IVariable selectedVariable = referenceProp.boxedValue as IVariable;

                RegisterValidVars();
                void RegisterValidVars()
                {
                    var dataAttr = varData.GetType().GetCustomAttribute<VariableDataAttribute>();
                    if (dataAttr == null)
                    {
                        Debug.LogWarning($"VariableDataAttribute for {varData.GetType().Name} not found. May be sign of underlying problem.");
                        return;
                    }
                    var contentType = dataAttr.ContentType;
                    validVarLookup.Clear();
                    validVarLookup.Add("<Value>", null); // Option to switch back to literal value

                    RegisterLocalVars();
                    void RegisterLocalVars()
                    {
                        IList<IVariable> validLocalVars = localFlowchart.Variables
                            .Where(elem => elem.ContentType.Equals(contentType))
                            .ToList();
                        for (int i = 0; i < validLocalVars.Count; i++)
                        {
                            var elem = validLocalVars[i];
                            if (!validVarLookup.ContainsKey(elem.Key))
                            {
                                validVarLookup.Add(elem.Key, elem);
                            }
                            else
                            {
                                Debug.LogWarning($"Variable key collision when trying to add variable {elem.Key} to the dropdown for {varDataProp.propertyPath}. " +
                                    $"There is already a variable with that key in the dropdown. Skipping this one.");
                            }

                            index++;

                            // Compare semantically (ItemID, Key, or reference). This handles both VariablePointer wrappers
                            // and direct Muscariable instances.
                            if (selectedVariable != null && VariablesSemanticallyEqual(selectedVariable, elem)) 
                            {
                                selectedIndex = index;
                                Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                            }
                        }
                    }

                    RegisterVarsFromOtherFlowcharts();
                    void RegisterVarsFromOtherFlowcharts()
                    {
                        IList<Flowchart> otherFlowchartsInScene = Flowchart.CachedFlowcharts.Where
                            ((elem) => elem != localFlowchart).ToList();
                        for (int i = 0; i < otherFlowchartsInScene.Count; i++)
                        {
                            var otherChart = otherFlowchartsInScene[i];
                            IList<IVariable> validVarsInOtherChart = otherChart.Variables
                                .Where(elem => elem.ContentType.Equals(contentType) && elem.Scope == VariableScope.Public)
                                .ToList();
                            for (int j = 0; j < validVarsInOtherChart.Count; j++)
                            {
                                var elem = validVarsInOtherChart[j];
                                string namespacedKey = $"{otherChart.gameObject.name}/{elem.Key}";
                                if (!validVarLookup.ContainsKey(namespacedKey))
                                {
                                    validVarLookup.Add(namespacedKey, elem);
                                }
                                else
                                {
                                    Debug.LogWarning($"Variable key collision when trying to add variable {namespacedKey} to the dropdown for {varDataProp.propertyPath}. " +
                                        $"There is already a variable with that key in the dropdown. Skipping this one.");
                                }

                                index++;

                                if (selectedVariable != null && VariablesSemanticallyEqual(selectedVariable, elem))
                                {
                                    selectedIndex = index;
                                    Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                                }
                            }
                        }
                    }

                    RegisterGlobalVars();
                    void RegisterGlobalVars()
                    {
                        IList<IVariable> globalVars = AmanitaManager.S.GlobalVariables;
                        IList<IVariable> validGlobalVars = globalVars
                            .Where(elem => elem.ContentType.Equals(contentType))
                            .ToList();

                        for (int i = 0; i < validGlobalVars.Count; i++)
                        {
                            var elem = validGlobalVars[i];
                            string namespacedKey = $"Global/{elem.Key}";
                            if (!validVarLookup.ContainsKey(namespacedKey))
                            {
                                validVarLookup.Add(namespacedKey, elem);
                            }
                            else
                            {
                                Debug.LogWarning($"Variable key collision when trying to add variable " +
                                    $"{namespacedKey} to the dropdown for {varDataProp.propertyPath}. " +
                                    $"There is already a variable with that key in the dropdown. " +
                                    $"Skipping this one.");
                            }

                            index++;

                            if (selectedVariable != null && VariablesSemanticallyEqual(selectedVariable, elem))
                            {
                                selectedIndex = index;
                                Debug.Log($"Found selected variable {elem.Key} at index " +
                                    $"{selectedIndex} in dropdown for {varDataProp.propertyPath}");
                            }
                        }
                    }
                }

                bool noVarsFound = validVarLookup.Count == 0;
                if (noVarsFound)
                {
                    return;
                }
                IList<string> options = validVarLookup.Keys.ToList();
                int prevSelectedIndex = Mathf.Min(options.Count, selectedIndex);
                
                IVariable chosenBefore = validVarLookup[options[prevSelectedIndex]];

                if (!shouldDrawLiteral && chosenBefore != null)
                {
                    popupRect = wholeFieldRect;
                }

                selectedIndex = EditorGUI.Popup(popupRect, selectedIndex, options.ToArray());
                
                if (selectedIndex != prevSelectedIndex)
                {
                    Debug.Log($"Selected something else");
                }

                IVariable chosenNow = validVarLookup[options[selectedIndex]];

                referenceProp.AssignVarRef(chosenNow, varData.ContentType);

            }

            EditorGUI.indentLevel = prevIndent;

            EditorGUI.EndProperty();
        }

        protected IDictionary<string, IVariable> validVarLookup = new Dictionary<string, IVariable>();
        protected UnityObj _variableSourceContext;

        protected virtual UnityObj GetBindingTarget(IVariable variable)
        {
            if (variable is UnityObj unityObj)
                return unityObj; // Legacy variable

            return FindPersistentHolderFor(variable, _variableSourceContext);
        }

        protected virtual MuscariableHolder FindPersistentHolderFor(IVariable variable, UnityObj context)
        {
            if (context == null || _assetResolver == null) return null;

            var path = _assetResolver.GetAssetPath(context);

            // Use resolver to enumerate holders (testable / mockable)
            IList<MuscariableHolder> holders = _assetResolver.LoadAllAssetsAtPath<MuscariableHolder>(path)
                .OfType<MuscariableHolder>()
                .ToList();

            Debug.Log($"[FindPersistentHolderFor] Searching holders for var key='{variable?.Key}' itemID={variable?.ItemID} at assetPath='{path}'. holders.Count={holders.Count}");

            LogDiscoveredHoldersForDiagnostings();
            void LogDiscoveredHoldersForDiagnostings()
            {
                for (int i = 0; i < holders.Count; i++)
                {
                    var holderElem = holders[i];
                    int innerHash = 0;
                    string innerKey = "(null)";
                    try
                    {
                        if (holderElem.Inner != null)
                        {
                            innerHash = RuntimeHelpers.GetHashCode(holderElem.Inner);
                            innerKey = holderElem.Inner.Key;
                        }
                    }
                    catch { /* ignore */ }
                    Debug.Log($"[FindPersistentHolderFor] holder[{i}] name='{holderElem.name}' instanceId={holderElem.GetInstanceID()} itemID={holderElem.ItemID} innerKey='{innerKey}' innerHash={innerHash}");
                }
            }

            // 1) Prefer matching by stable ItemID (survives domain reloads)
            if (variable != null)
            {
                var byId = holders.FirstOrDefault(elem => elem.ItemID == variable.ItemID);
                if (byId != null)
                {
                    Debug.Log($"[FindPersistentHolderFor] Matched by ItemID: holder name='{byId.name}' instanceId={byId.GetInstanceID()} -> var key='{variable.Key}' itemID={variable.ItemID}");
                    return byId;
                }
            }

            // 2) Fallback: try matching by the Inner reference (existing behavior)
            foreach (var elem in holders)
            {
                try
                {
                    if (elem.Inner == variable)
                    {
                        Debug.Log($"[FindPersistentHolderFor] Matched by Inner reference: holder name='{elem.name}' instanceId={elem.GetInstanceID()} -> var key='{variable?.Key}'");
                        return elem;
                    }
                }
                catch
                {
                    var inner = elem.Inner;
                    if (inner == variable)
                    {
                        Debug.Log($"[FindPersistentHolderFor] (fallback) Matched by Inner reference: holder name='{elem.name}' instanceId={elem.GetInstanceID()} -> var key='{variable?.Key}'");
                        return elem;
                    }
                }
            }

            Debug.Log($"[FindPersistentHolderFor] No holder found for var key='{variable?.Key}' itemID={variable?.ItemID} at assetPath='{path}'");
            return null;
        }


        protected virtual bool VarRefPropHasAnythingAssigned(SerializedProperty varRefProp)
        {
            bool result = false;

            switch (varRefProp.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    // UnityEngine.Object or ScriptableObject-backed variable
                    result = varRefProp.objectReferenceValue != null;
                    break;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                    // [SerializeReference] polymorphic variable
                    result = varRefProp.managedReferenceValue != null;
                    break;

                default:
                    Debug.LogError($"[VarRefPropHasAnythingAssigned] Did not account for var ref prop being of serialized property type {varRefProp.propertyType}");
                    break;
            }

            return result;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var referenceProp = property.FindPropertyRelative("varRef");
            if (referenceProp != null && referenceProp.propertyType == SerializedPropertyType.ManagedReference)
            {
                // Let Unity calculate height for polymorphic managed refs
                return EditorGUI.GetPropertyHeight(referenceProp, true);
            }
            return EditorGUIUtility.singleLineHeight;
        }

        // Helper: compare two IVariable instances semantically so we can detect the currently-selected
        // variable regardless of whether the stored reference is a VariablePointer wrapper or a raw Muscariable.
        private static bool VariablesSemanticallyEqual(IVariable a, IVariable b)
        {
            if (a == null || b == null) return false;

            try
            {
                // Prefer stable ItemID when available (non-zero)
                if (a.ItemID != 0 || b.ItemID != 0)
                {
                    if (a.ItemID == b.ItemID) return true;
                }
            }
            catch { /* ignore */ }

            try
            {
                if (!string.IsNullOrEmpty(a.Key) && a.Key == b.Key) return true;
            }
            catch { /* ignore */ }

            // If a is a pointer type, compare against its underlying component if possible
            if (a is IVariablePointer ptr)
            {
                var compVar = ptr.Component as IVariable;
                if (compVar != null)
                {
                    if (VariablesSemanticallyEqual(compVar, b)) return true;
                }
            }

            // Reference equality fallback
            return object.ReferenceEquals(a, b);
        }
    }



    [CustomPropertyDrawer(typeof(BooleanData))]
    public class BooleanDataDrawer : VariableDataDrawer<BooleanVariable>
    { }

    [CustomPropertyDrawer(typeof(IntegerData))]
    public class IntegerDataDrawer : VariableDataDrawer<IntegerVariable>
    { }

    [CustomPropertyDrawer(typeof(FloatData))]
    public class FloatDataDrawer : VariableDataDrawer<FloatVariable>
    { }

    [CustomPropertyDrawer(typeof(StringData))]
    public class StringDataDrawer : VariableDataDrawer<StringVariable>
    { }

    [CustomPropertyDrawer(typeof(StringDataMulti))]
    public class StringDataMultiDrawer : VariableDataDrawer<StringVariable>
    { }
}