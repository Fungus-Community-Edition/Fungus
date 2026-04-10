using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;
using UnityObj = UnityEngine.Object;
using Type = System.Type;
using System.Reflection;

namespace AtMycelia.Hyphlow.EditorUtils
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
        protected static IReadOnlyList<Type> LegacyTypes
        {
            get
            {
                if (legacyTypes == null || legacyTypes.Count == 0)
                {
                    RefreshVariableTypeCache();
                }
                return legacyTypes;
            }
        }

        protected static IReadOnlyList<Type> MuscariTypes
        {
            get
            {
                if (muscariTypes == null || muscariTypes.Count == 0)
                {
                    RefreshVariableTypeCache();
                }
                return muscariTypes;
            }
        }

        /// <summary>
        /// Flowchart in context for adding variables. (Set by DoAddVariable / legacy menu path.)
        /// </summary>
        protected static Flowchart curFlowchart;
        protected static IMuscariableSource curSource;

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
            legacyTypes = VariableTypeRegistry.AllLegacyTypes.Where(ShouldBeShownInMenu).ToList();
            muscariTypes = VariableTypeRegistry.AllMuscariableTypes.Where(ShouldBeShownInMenu).ToList();
            allTypes = legacyTypes.Concat(muscariTypes).ToList();
            bool ShouldBeShownInMenu(Type varType)
            {
                bool result = false;
                VariableInfoAttribute attr = varType.GetCustomAttribute<VariableInfoAttribute>();
                if (attr != null)
                {
                    result = attr.ShowInMenu;
                }
                return result;
            }
        }

        // Cached list of concrete variable component types (legacy Variable system)
        protected static IReadOnlyList<Type> legacyTypes, muscariTypes, allTypes;

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
            IList<Type> contentTypesPreparedFor = new List<Type>();

            // Check for muscariables first
            PrepareItemsFor(MuscariTypes);
            PrepareItemsFor(LegacyTypes);

            void PrepareItemsFor(IReadOnlyList<Type> varTypes)
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
                                         Flowchart toAddVarTo)
        {
            curFlowchart = toAddVarTo;
            curSource = toAddVarTo;

            if (!HyphlowEditorPreferences.useLegacyMenus)
            {
                var win = new VariableSelectPopupWindowContent(currentHandlerName, POPUP_WIDTH, POPUP_HEIGHT);
                PopupWindow.Show(position, win);
            }

            // Always build / show the legacy menu (mirrors CommandSelector pattern).
            ShowLegacyMenu(curFlowchart);
        }

        protected const int POPUP_WIDTH = 200;
        protected const int POPUP_HEIGHT = 200;

        public static void DoAddVariable(Rect position, string currentHandlerName,
            IMuscariableSource toAddVarTo, System.Action onVarAdded = null)
        {
            curFlowchart = null;
            curSource = toAddVarTo;

            if (!HyphlowEditorPreferences.useLegacyMenus)
            {
                var win = new VariableSelectPopupWindowContent(currentHandlerName, POPUP_WIDTH, POPUP_HEIGHT);
                PopupWindow.Show(position, win);
            }

            // Always build / show the legacy menu (mirrors CommandSelector pattern).
            ShowLegacyMenu(toAddVarTo);
        }
        #endregion

        #region Legacy Menu (Context GenericMenu)

        /// <summary>
        /// Build and show the old non-searchable menu variant.
        /// </summary>
        protected static void ShowLegacyMenu(Flowchart flowchart)
        {
            GenericMenu menu = PrepMenu(allTypes);
            menu.ShowAsContext();
        }

        private static GenericMenu PrepMenu(IReadOnlyList<Type> varTypes)
        {
            GenericMenu menu = new GenericMenu();

            IList<Type> typesWithCategory = varTypes.Where(TypeHasCategory).ToList();
            static bool TypeHasCategory(Type type)
            {
                var info = VariableEditor.GetVariableInfo(type);
                return info == null || !string.IsNullOrEmpty(info.Category);
            }

            IList<Type> uncategorized = varTypes.Where((elem) => !TypeHasCategory(elem)).ToList();

            // We want to list the uncategorized types first
            AddToMenu(uncategorized);
            void AddToMenu(IList<Type> typesToAdd)
            {
                foreach (var typeEl in typesToAdd)
                {
                    var info = VariableEditor.GetVariableInfo(typeEl);
                    string displayLabel = MakeDisplayLabel(info);
                    menu.AddItem(new GUIContent(displayLabel), false, AddVariable, typeEl);
                }
            }
            AddToMenu(typesWithCategory);
            return menu;
        }

        public static void ShowLegacyMenu(IVariableSource variableSource)
        {
            GenericMenu menu = PrepMenu(muscariTypes);
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

        public static void AddVariableToSource(object obj)
        {

        }

        private static UnityObj ResolveRecordTarget()
        {
            if (curSource is Flowchart flowchart)
            {
                VariableManagerComponent manager = flowchart.GetComponent<VariableManagerComponent>();
                if (manager != null)
                {
                    return manager;
                }

                return flowchart;
            }

            return curSource as UnityObj;
        }

        private static void MarkDirty(UnityObj target)
        {
            if (target == null)
            {
                return;
            }

            EditorUtility.SetDirty(target);

            if (target is Component component && component.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
            }

            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
        }

        /// <summary>
        /// Creates a new Variable of the supplied type on the active flowchart.
        /// Optionally attempts to place it after an existing variable with the suggested name.
        /// </summary>
        /// <param name="varTypeToAdd">Type expected.</param>
        /// <param name="suggestedName">Optional preferred key (used also to attempt positional insertion).</param>
        public static void AddVariable(object varTypeToAdd, string suggestedName)
        {
            if (varTypeToAdd is not Type variableType)
            {
                return;
            }

            UnityObj varSourceObj = ResolveRecordTarget();
            if (varSourceObj == null || curSource == null)
            {
                return;
            }

            // We'll assume that the var type passed is a Muscariable instead of a legacy type
            VariableInfoAttribute info = VariableEditor.GetVariableInfo(variableType);
            Type muscariType = typeof(Muscariable);
            if (info == null || !muscariType.IsAssignableFrom(variableType))
            {
                Debug.LogError($"Type {variableType.Name} is not a Muscariable or does not " +
                    $"have a VariableInfo attribute.");
                return;
            }

            string typeName = info.ContentType.Name;
            if (typeName.Equals("Single"))
            {
                typeName = "Float";
            }

            Undo.RecordObject(varSourceObj, $"Add {typeName} Variable");
            curSource.AddNewVariableOfContentType(info.ContentType, suggestedName);
            MarkDirty(varSourceObj);
        }

        #endregion
    }
}