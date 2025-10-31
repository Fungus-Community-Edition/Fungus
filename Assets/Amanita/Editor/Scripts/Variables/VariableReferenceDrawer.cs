using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Custom drawer for the VariableReference, allows for selecting a target variable.
    /// Supports both legacy Variable (MonoBehaviour) and Muscariable (SerializeReference IVariable).
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null)
            {
                Debug.LogWarning("VariableReferenceDrawer ONGUI has no property to work with. Exiting early.");
                return;
            }

            if (property.serializedObject == null || property.serializedObject.targetObject == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Target lost"));
                return;
            }

            var beginLabel = EditorGUI.BeginProperty(position, label, property);
            var startPos = position;
            position = EditorGUI.PrefixLabel(position, beginLabel);
            position.height = EditorGUIUtility.singleLineHeight;

            // Prefer managed IVariable path; legacy fallback supported
            var managedVarProp = property.FindPropertyRelative("variable");
            var legacyVarProp = property.FindPropertyRelative("legacyVariable");

            if (managedVarProp == null && legacyVarProp == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Invalid VariableReference (missing fields)"));
                EditorGUI.EndProperty();
                return;
            }

            bool isManaged = managedVarProp != null &&
                             managedVarProp.propertyType == SerializedPropertyType.ManagedReference;

            Variable legacyVar = null;
            IVariable selectedIVar = null;

            if (isManaged)
            {
                selectedIVar = managedVarProp.managedReferenceValue as IVariable;
            }
            else if (legacyVarProp != null)
            {
                legacyVar = legacyVarProp.objectReferenceValue as Variable;
            }

            // Auto-detect owning flowchart once
            if (lastFlowchart == null)
            {
                if (isManaged && selectedIVar != null)
                {
                    var owner = ResolveOwnerFlowchart(selectedIVar);
                    if (owner != null) lastFlowchart = owner;
                }
                else if (!isManaged && legacyVar != null)
                {
                    lastFlowchart = legacyVar.GetComponent<Flowchart>();
                }
            }

            // Flowchart selector
            lastFlowchart = EditorGUI.ObjectField(position, lastFlowchart, typeof(Flowchart), true) as Flowchart;
            position.y += EditorGUIUtility.singleLineHeight;

            // If managed reference selected and it belongs to a different flowchart and is Private, clear it.
            if (isManaged && selectedIVar != null && lastFlowchart != null)
            {
                var owner = ResolveOwnerFlowchart(selectedIVar);
                if (owner != null &&
                    !ReferenceEquals(owner, lastFlowchart) &&
                    selectedIVar.Scope == VariableScope.Private)
                {
                    managedVarProp.managedReferenceValue = null;
                    selectedIVar = null;
                }
            }

            if (lastFlowchart != null)
            {
                var popupRect = startPos;
                popupRect.y = position.y;

                string typeName = isManaged
                    ? (selectedIVar != null ? selectedIVar.GetType().Name : "No Var Selected")
                    : (legacyVar != null ? legacyVar.GetType().Name : "No Var Selected");

                var prefixLabel = new GUIContent(typeName);
                EditorGUI.indentLevel++;
                var propToEdit = isManaged ? managedVarProp : legacyVarProp;

                VariableEditor.VariableField(
                    propToEdit,
                    prefixLabel,
                    lastFlowchart,
                    "<None>",
                    null,
                    (popupLabel, selectedIndex, displayedOptions) =>
                        EditorGUI.Popup(popupRect, popupLabel, selectedIndex, displayedOptions)
                );
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.PrefixLabel(position, new GUIContent("Flowchart Required"));
            }

            // Commit changes defensively
            managedVarProp?.serializedObject?.ApplyModifiedProperties();
            legacyVarProp?.serializedObject?.ApplyModifiedProperties();
            property.serializedObject?.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }

        private static Flowchart ResolveOwnerFlowchart(IVariable varToCheckFor)
        {
            if (varToCheckFor == null) return null;

            // Direct owner
            if (varToCheckFor.Owner is Flowchart fOwner) return fOwner;

            // Parent link on Muscariable
            if (varToCheckFor is Muscariable m && m.ParentFlowchart != null) return m.ParentFlowchart;

            var list = Flowchart.CachedFlowcharts;

            // Prefer ItemId + Key match to avoid cross-flow collisions (ItemId restarts per Flowchart)
            try
            {
                int id = varToCheckFor.ItemId;
                string key = varToCheckFor.Key;

                if (id != 0 && !string.IsNullOrEmpty(key))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var fChart = list[i];
                        var match = fChart.Variables.FirstOrDefault(varEl =>
                            varEl != null &&
                            varEl.ItemId == id &&
                            !string.IsNullOrEmpty(varEl.Key) &&
                            varEl.Key == key);
                        if (match != null) return fChart;
                    }
                }
            }
            catch { /* ignore */ }

            // Fallback: uniquely match by Key across flowcharts (only if unique)
            try
            {
                if (!string.IsNullOrEmpty(varToCheckFor.Key))
                {
                    Flowchart unique = null;
                    int hits = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var fChart = list[i];
                        bool has = fChart.Variables.Any(varEl => varEl != null && varEl.Key == varToCheckFor.Key);
                        if (has)
                        {
                            unique = fChart;
                            hits++;
                            if (hits > 1) break;
                        }
                    }
                    if (hits == 1) return unique;
                }
            }
            catch { /* ignore */ }

            return null;
        }
    }
}