using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Searchable popup window content used to add a Variable component to the current Flowchart.
    /// Mirrors CommandSelectorPopupWindowContent pattern.
    /// </summary>
    public class VariableSelectPopupWindowContent : BasePopupWindowContent
    {
        /// <summary>
        /// All variable types available for user selection. Lazily (re)cached.
        /// </summary>
        protected static IReadOnlyList<System.Type> LegacyTypes
        {
            get
            {
                if (_legacyTypes == null || _legacyTypes.Count == 0)
                {
                    RefreshVariableTypeCache();
                }
                return _legacyTypes;
            }
        }

        protected static IReadOnlyList<System.Type> MuscariTypes
        {
            get
            {
                if (_muscariTypes == null || _muscariTypes.Count == 0)
                {
                    RefreshVariableTypeCache();
                }
                return _muscariTypes;
            }
        }

        /// <summary>
        /// Flowchart in context for adding variables. (Set by DoAddVariable / legacy menu path.)
        /// </summary>
        protected static Flowchart curFlowchart;

        #region Lifecycle & Caching

        [UnityEditor.Callbacks.DidReloadScripts]
        protected static void OnScriptsReloaded()
        {
            RefreshVariableTypeCache();
        }

        /// <summary>
        /// Refresh the cached list of variable types from the registry.
        /// </summary>
        protected static void RefreshVariableTypeCache()
        {
            // Using registry instead of reflection scan for performance / determinism.
            _legacyTypes = VariableTypeRegistry.AllLegacyTypes;
            _muscariTypes = VariableTypeRegistry.AllMuscariableTypes;
        }

        // Cached list of concrete variable component types (legacy Variable system)
        protected static IReadOnlyList<System.Type> _legacyTypes, _muscariTypes;

        #endregion

        #region Construction

        public VariableSelectPopupWindowContent(string currentHandlerName, int width, int height)
            : base(currentHandlerName, width, height)
        {
        }

        #endregion

        #region BasePopupWindowContent Overrides

        /// <summary>
        /// Populate the internal list for filtering / display.
        /// </summary>
        protected override void PrepareAllItems()
        {
            // Iterate with index so we can map back to original type directly.
            IList<System.Type> contentTypesPreparedFor = new List<System.Type>();

            // Check for muscariables first
            PrepareItemsFor(MuscariTypes);
            PrepareItemsFor(LegacyTypes);

            void PrepareItemsFor(IReadOnlyList<System.Type> varTypes)
            {
                for (int typeIndex = 0; typeIndex < varTypes.Count; typeIndex++)
                {
                    var type = varTypes[typeIndex];
                    var info = VariableEditor.GetVariableInfo(type);
                    if (info == null)
                    {
                        string logMessage = $"Type {type.Name} does not have a variable info attribute.";
                        Debug.LogWarning(logMessage);
                        continue;
                    }

                    if (contentTypesPreparedFor.Contains(info.ContentType))
                    {
                        continue;
                    }

                    // We're not going to worry about any of the types having an ObsoleteAttribute
                    string display = MakeDisplayLabel(info);

                    // The original index into VariableTypes is preserved in 'typeIndex'.
                    allItems.Add(new FilteredListItem(typeIndex, display));
                }
            }
        }

        protected static string MakeDisplayLabel(VariableInfoAttribute info)
        {
            string result;
            if (info.Category.Length > 0)
            {
                result = string.Format(_displayLabelFormat, info.Category, info.OptionDisplayName);
                
            }
            else
            {
                result = info.OptionDisplayName;
            }

            return result;
        }

        protected static string _displayLabelFormat = "{0}/{1}";

        /// <summary>
        /// Called when user confirms a selection (keyboard enter, double click, etc).
        /// </summary>
        protected override void SelectByOrigIndex(int index)
        {
            if (index < 0 || index >= LegacyTypes.Count)
                return;

            AddVariable(LegacyTypes[index]);
        }

        #endregion

        #region Public Entry Points

        /// <summary>
        /// Show variable add popup (new searchable version or legacy menu fallback).
        /// </summary>
        /// <param name="position">Anchor rect (button rect).</param>
        /// <param name="currentHandlerName">Optional pre-filter / search seed.</param>
        /// <param name="toAddVarTo">Target flowchart to add variable to.</param>
        /// <param name="onVarAdded">Optional callback after addition (currently unused).</param>
        public static void DoAddVariable(Rect position,
                                         string currentHandlerName,
                                         Flowchart toAddVarTo,
                                         System.Action onVarAdded = null)
        {
            curFlowchart = toAddVarTo;

            if (!AmanitaEditorPreferences.useLegacyMenus)
            {
                var win = new VariableSelectPopupWindowContent(currentHandlerName, POPUP_WIDTH, POPUP_HEIGHT);
                PopupWindow.Show(position, win);
            }

            // Always build / show the legacy menu (mirrors CommandSelector pattern).
            ShowLegacyMenu(toAddVarTo);
        }

        protected const int POPUP_WIDTH = 200;
        protected const int POPUP_HEIGHT = 200;

        #endregion

        #region Legacy Menu (Context GenericMenu)

        /// <summary>
        /// Build and show the old non-searchable menu variant.
        /// </summary>
        protected static void ShowLegacyMenu(Flowchart flowchart)
        {
            GenericMenu menu = new GenericMenu();

            IList<System.Type> typesWithCategory = _legacyTypes.Where(TypeHasCategory).ToList();
            static bool TypeHasCategory(System.Type type)
            {
                var info = VariableEditor.GetVariableInfo(type);
                return info == null || !string.IsNullOrEmpty(info.Category);
            }
            IList<System.Type> uncategorized = _legacyTypes.Where((elem) => !TypeHasCategory(elem)).ToList();

            // We want to list the uncategorized types first
            AddToMenu(uncategorized);
            void AddToMenu(IList<System.Type> typesToAdd)
            {
                foreach (var typeEl in typesToAdd)
                {
                    var info = VariableEditor.GetVariableInfo(typeEl);
                    string displayLabel = MakeDisplayLabel(info);
                    menu.AddItem(new GUIContent(displayLabel), false, AddVariable, typeEl);
                }
            }
            AddToMenu(typesWithCategory);

            menu.ShowAsContext();
        }

        #endregion

        #region Variable Creation

        /// <summary>
        /// Convenience overload for GenericMenu callback signature.
        /// </summary>
        public static void AddVariable(object obj)
        {
            AddVariable(obj, string.Empty);
        }

        /// <summary>
        /// Creates a new Variable component of the supplied type on the active flowchart.
        /// Optionally attempts to place it after an existing variable with the suggested name.
        /// </summary>
        /// <param name="varToAdd">System.Type expected.</param>
        /// <param name="suggestedName">Optional preferred key (used also to attempt positional insertion).</param>
        public static void AddVariable(object varToAdd, string suggestedName)
        {
            if (varToAdd is not System.Type variableType)
                return;

            var flowchart = curFlowchart != null ? curFlowchart : FlowchartWindow.GetFlowchart();
            if (flowchart == null)
            {
                Debug.LogWarning("No Flowchart available to add variable to.");
                return;
            }

            Undo.RecordObject(flowchart, "Add Variable");

            // Add component instance
            var newVariable = flowchart.gameObject.AddComponent(variableType) as Variable;
            if (newVariable == null)
            {
                Debug.LogError($"Failed to add variable component of type {variableType.Name} to {curFlowchart.name}");
                return;
            }

            // Determine unique key
            newVariable.Key = flowchart.GetUniqueVariableKey(suggestedName);

            // If the suggested name exists, insert after that variable; otherwise append.
            if (!string.IsNullOrEmpty(suggestedName))
            {
                var existingVariable = flowchart.GetVariable(suggestedName);
                if (existingVariable != null)
                {
                    var listCopy = new List<IVariable>(flowchart.Variables);
                    int insertionIndex = listCopy.IndexOf(existingVariable) + 1;
                    flowchart.InsertVariable(insertionIndex, newVariable);
                }
                else
                {
                    flowchart.AddVariable(newVariable);
                }
            }
            else
            {
                flowchart.AddVariable(newVariable);
            }

            // Ensure prefab instances properly record the new component state.
            PrefabUtility.RecordPrefabInstancePropertyModifications(flowchart);
        }

        #endregion
    }
}