using Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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

            PrepEventListeners();
            void PrepEventListeners()
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

                // By responding to the changes in the UI this way, we can ensure that
                // undo operations work as expected. Because again: we are not going to
                // rely on UITK's built-in binding system for this.
                AmanitaEditorSignals.KeyFieldChanged += OnKeyFieldChanged;
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

                AmanitaEditorSignals.KeyFieldChanged -= OnKeyFieldChanged;
                AmanitaEditorSignals.ScopeFieldChanged -= OnScopeFieldChanged;
                AmanitaEditorSignals.ValueFieldChanged -= OnValueFieldChanged;
                subsActive = false;
            }
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
            // In the future, due to how Flowchart implements IVariableSource, we might
            // want just a single DoAddVariable method that takes IVariableSource.
            if (Flowchart != null)
            {
                VariableSelectPopupWindowContent.DoAddVariable(rect, "", Flowchart);
            }
            else if (variableSource is IReorderableMuscariableSource muscaSource)
            {
                VariableSelectPopupWindowContent.DoAddVariable(rect, "", muscaSource);
            }
        }

        protected bool subsActive = false;

        protected bool HasLiveVariableSourceReference()
        {
            if (variableSource == null)
            {
                return false;
            }

            if (variableSource is UnityObj unityObj)
            {
                return unityObj != null;
            }

            return true;
        }

        protected void EnsureVariableSource(IVariableSource ownerCandidate)
        {
            if (_isDisposed || HasLiveVariableSourceReference())
            {
                return;
            }

            if (TryRebindTo(ownerCandidate))
            {
                return;
            }

#if UNITY_EDITOR
            var flowchartFromWindow = FlowchartWindow.GetFlowchart();
            TryRebindTo(flowchartFromWindow);
#endif
        }

        protected bool TryRebindTo(IVariableSource candidate)
        {
            if (candidate is not IReorderableVariableSource reorderable)
            {
                return false;
            }

            return TryRebindTo(reorderable);
        }

        protected bool TryRebindTo(IReorderableVariableSource newSource)
        {
            if (newSource == null || _isDisposed)
            {
                return false;
            }

            if (ReferenceEquals(variableSource, newSource) && HasLiveVariableSourceReference())
            {
                return true;
            }

            ToggleSubs(false);
            variableSource = newSource;
            ToggleSubs(true);
            Refresh();
            return HasLiveVariableSourceReference();
        }

        protected UnityObj ResolveRecordTarget(IVariable variable)
        {
            if (variable == null)
            {
                return null;
            }

            UnityObj direct = variable as UnityObj;
            if (direct != null)
            {
                return direct;
            }

            if (variable.Owner is UnityObj ownerObj && ownerObj != null)
            {
                return ownerObj;
            }

            if (variableSource is UnityObj sourceObj && sourceObj != null)
            {
                return sourceObj;
            }

            return null;
        }
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

            if (varInvolved is UnityObj legacyVar && legacyVar != null)
            {
                Debug.Log($"Removing legacy variable asset: {varInvolved.Key}");
                UnityObj.DestroyImmediate(legacyVar);
            }
        }

        protected virtual void OnKeyFieldChanged(VariableRow rowInvolved, string newKey)
        {
            if (!WeAreManaging(rowInvolved))
            {
                return;
            }

            IVariable theVar = rowInvolved.VarToRepresent;
            if (theVar.Key != newKey)
            {
                RecordAndApplyChange(theVar, "Key", (varToChange) => varToChange.Key = newKey);
            }
        }

        protected virtual bool WeAreManaging(VariableRow row)
        {
            if (row == null || row.VarToRepresent == null)
            {
                return false;
            }

            IVariable variable = row.VarToRepresent;
            IVariableSource owner = variable.Owner;

            if (!HasLiveVariableSourceReference())
            {
                EnsureVariableSource(owner);
            }

            if (owner == null && HasLiveVariableSourceReference())
            {
                variable.Owner = variableSource;
                owner = variableSource;
            }

            return owner != null && variableSource != null && ReferenceEquals(owner, variableSource);
        }

        protected void RecordAndApplyChange(IVariable variable, string description, Action<IVariable> applyChange)
        {
            if (variable == null || applyChange == null)
            {
                return;
            }

            EnsureVariableSource(variable.Owner);

            string varType = variable.GetType().Name;
            UnityObj toRecord = ResolveRecordTarget(variable);
            if (toRecord == null)
            {
                Debug.LogError($"VariableRowManager could not resolve a UnityEngine.Object to record for {varType} {description}.");
                return;
            }

            Undo.RecordObject(toRecord, $"Change to {varType} {description}");

            applyChange(variable);

            if (!HasLiveVariableSourceReference())
            {
                EnsureVariableSource(variable.Owner);
            }

            if (variableSource is ScriptableObject so)
            {
                so.MarkDirtyAndSave();
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
                RecordAndApplyChange(theVar, "Scope", (varToChange) => varToChange.Scope = scope);
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
                RecordAndApplyChange(theVar, "Value", (varToChange) => varToChange.BoxedValue = newVal);
            }
        }

        #endregion

        #region Refresh APIs
        public void Refresh()
        {
            if (_isDisposed || variableSource == null || _listView == null)
                return;

            _listView.SetVariables(variableSource.Variables);
        }
        #endregion

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

        public virtual void ReleaseRowsFromList()
        {
            _listView?.Clear();
        }

        #endregion
    }
}
