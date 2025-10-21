using Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Amanita.TextVariationHandler.Section;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRowManager : IDisposable
    {
        public virtual void Init(VRowManagerInitArgs initArgs)
        {
            _isDisposed = false;

            bool allWentWell;
            ValidateArgs();
            void ValidateArgs()
            {
                int errorLogs = 0;
                if (initArgs == null)
                {
                    Debug.LogError("VariableRowManager was given a null args object.");
                    allWentWell = false;
                    return;
                }

                if (initArgs.VariableSource == null)
                {
                    Debug.LogError("VariableRowManager was not given a Flowchart to work with.");
                    errorLogs++;
                }

                if (initArgs.VariableListView == null)
                {
                    Debug.LogError($"VariableRowManager was not given a list view to work with.");
                    errorLogs++;
                }

                if (initArgs.Root == null)
                {
                    Debug.LogError("VariableRowManager was not given a root to work with.");
                    errorLogs++;
                }

                if (initArgs.AddButton == null)
                {
                    Debug.LogError("VariableRowManager was not given an add button to work with");
                    errorLogs++;
                }

                allWentWell = errorLogs == 0;
            }

            if (!allWentWell)
            {
                Debug.LogError("Failed to initialize VariableRowManager.");
                return;
            }

            PrepListView();
            void PrepListView()
            {
                _listView = initArgs.VariableListView;
            }

            InitVisuals(initArgs);
            void InitVisuals(VRowManagerInitArgs initArgs)
            {
                Root = initArgs.Root;
                _addButton = initArgs.AddButton;
            }

            PrepFcEventListeners();
            void PrepFcEventListeners()
            {
                ToggleSubs(false);
                variableSource = initArgs.VariableSource;
                ToggleSubs(true);
            }

            Refresh();
        }

        protected bool _isDisposed;
        protected IReorderableVariableSource variableSource;
        protected Flowchart Flowchart => variableSource as Flowchart;
        protected IVariableListView _listView;
        protected Button _addButton;

        public VisualElement Root { get; protected set; }

        #region Event Wiring / Visual Init
        protected virtual void ToggleSubs(bool on)
        {
            if (variableSource == null || _listView == null)
            {
                return;
            }

            if (on && !subsActive)
            {
                variableSource.VariableAdded += OnVariableAdded;
                variableSource.VariableRemoved += OnVariableRemoved;
                _listView.OrderChanged += OnOrderChanged;
                _addButton.clicked += OnAddButtonClicked;
                AmanitaEditorSignals.VarRowRemoveButtonClicked += OnVarRowRemovalButtonClicked;
                AmanitaEditorSignals.KeyFieldChanged += OnKeyFieldFocusLost;
                AmanitaEditorSignals.ScopeFieldChanged += OnScopeFieldChanged;
                AmanitaEditorSignals.ValueFieldChanged += OnValueFieldChanged;
                subsActive = true;
            }
            else if (!on)
            {
                variableSource.VariableAdded -= OnVariableAdded;
                variableSource.VariableRemoved -= OnVariableRemoved;
                _listView.OrderChanged -= OnOrderChanged;
                _addButton.clicked -= OnAddButtonClicked;
                AmanitaEditorSignals.VarRowRemoveButtonClicked -= OnVarRowRemovalButtonClicked;
                AmanitaEditorSignals.KeyFieldChanged -= OnKeyFieldFocusLost;
                AmanitaEditorSignals.ValueFieldChanged -= OnValueFieldChanged;
                subsActive = false;
            }
        }

        protected virtual void OnScopeFieldChanged(VariableRow row, VariableScope scope)
        {
            if (!WeAreManaging(row))
            {
                return;
            }

            IVariable theVar = row.VarToRepresent;
            if (theVar.Scope != scope)
            {
                string varType = theVar.GetType().Name;
                Undo.RecordObject(variableSource as UnityObj, $"Changed {varType} Scope");
                theVar.Scope = scope;
            }
        }

        protected virtual void OnValueFieldChanged(VariableRow row, object newVal)
        {
            if (!WeAreManaging(row))
            {
                return;
            }

            IVariable theVar = row.VarToRepresent;
            if (!Equals(theVar.BoxedValue, newVal))
            {
                string varType = theVar.GetType().Name;
                Undo.RecordObject(variableSource as UnityObj, $"Changed {varType} Name");
                theVar.BoxedValue = newVal;
            }
        }

        // Why FocusLost instead of on any change? Because then we'd be responding to every keystroke;
        // we only want to write the key to the muscari when the user's done entering in the changed key
        protected virtual void OnKeyFieldFocusLost(VariableRow rowInvolved, string newKey)
        {
            if (!WeAreManaging(rowInvolved))
            {
                return;
            }

            IVariable theVar = rowInvolved.VarToRepresent;
            if (theVar.Key != newKey)
            {
                // Register an Undo
                string varType = theVar.GetType().Name;
                Undo.RecordObject(variableSource as UnityObj, $"Changed {varType} Key");
                theVar.Key = newKey;
            }
        }

        protected bool subsActive = false;
        #endregion

        #region Variable Event Handlers

        protected virtual void OnVarRowRemovalButtonClicked(VariableRow row)
        {
            if (!WeAreManaging(row))
            {
                return;
            }

            if (row == null || row.VarToRepresent == null)
            {
                string logMessage = "VariableRowManager was given a null VariableRow or VariableRow with " +
                    "null VarToRepresent.";
                Debug.LogError(logMessage);
                return;
            }

            IVariable varInvolved = row.VarToRepresent;
            var owner = varInvolved.Owner;
            owner.RemoveVariable(varInvolved);

            UnityObj destroyTarget = GetDestroyTarget(varInvolved);
            if (destroyTarget != null)
            {
                Debug.Log($"Destroying variable asset: {destroyTarget.name} ({destroyTarget.GetType().Name})");
                Undo.DestroyObjectImmediate(destroyTarget);
            }
            else
            {
                Debug.LogWarning($"Could not find a persistent asset to destroy for variable '{varInvolved.Key}'. " +
                    $"Type of the var itself: {varInvolved.GetType()}.");
            }

        }

        protected virtual bool WeAreManaging(VariableRow row) => row.VarToRepresent.Owner == variableSource;

        protected UnityObj GetDestroyTarget(IVariable variable)
        {
            if (variable is UnityObj unityObj)
                return unityObj; // Legacy variable

            // Muscariable path — find its holder in the variable source
            return FindPersistentHolderFor(variable);
        }

        protected static MuscariableHolder FindPersistentHolderFor(IVariable variable)
        {
            UnityObj context = variable.Owner as UnityObj;
            var path = AssetDatabase.GetAssetPath(context);

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var asset in subAssets)
            {
                if (asset is MuscariableHolder holder)
                {
                    if (holder.Inner == variable)
                        return holder;
                }
            }
            return null;
        }

        protected virtual void OnVariableAdded(IVariable added)
        {
            if (_isDisposed || added == null) return;
            _listView?.AddVariable(added);
        }

        protected virtual void OnVariableRemoved(IVariable removed)
        {
            if (_isDisposed || removed == null) return;
            _listView?.RemoveVariable(removed);
        }

        protected virtual void OnOrderChanged(IList<IVariable> newlyOrderedVars)
        {
            variableSource.ReorderVariables(newlyOrderedVars);
        }

        protected virtual void OnAddButtonClicked()
        {
            Rect rect = _addButton.worldBound;
            if (Flowchart != null)
            {
                VariableSelectPopupWindowContent.DoAddVariable(rect, "", Flowchart);
            }
            else if (variableSource is IReorderableMuscariableSource muscaSource)
            {
                VariableSelectPopupWindowContent.DoAddVariable(rect, "", muscaSource);
            }
            
        }

        #endregion

        #region Refresh APIs
        /// <summary>
        /// Full rebuild: just repopulates the itemsSource list on the ListView.
        /// </summary>
        public void Refresh()
        {
            if (_isDisposed || variableSource == null || _listView == null)
                return;

            _listView.SetVariables(variableSource.Variables);
        }
        #endregion

        public virtual void ReleaseRowsFromList()
        {
            // With virtualization, clearing variables triggers unbind & release logic
            _listView?.Clear();
        }

        #region Dispose
        public virtual void Dispose()
        {
            if (_isDisposed) return;

            ToggleSubs(false);
            ReleaseRowsFromList();

            _listView?.Dispose();

            _listView = null;
            variableSource = null;
            Root = null;
            _isDisposed = true;
        }

        #endregion
    }
}
