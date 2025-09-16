using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            if (on)
            {
                variableSource.VariableAdded += OnVariableAdded;
                variableSource.VariableRemoved += OnVariableRemoved;
                _listView.OrderChanged += OnOrderChanged;
                _addButton.clicked += OnAddButtonClicked;
            }
            else
            {
                variableSource.VariableAdded -= OnVariableAdded;
                variableSource.VariableRemoved -= OnVariableRemoved;
                _listView.OrderChanged -= OnOrderChanged;
                _addButton.clicked -= OnAddButtonClicked;
            }
        }

        #endregion

        #region Variable Event Handlers
        protected virtual void OnVariableAdded(IVariable added)
        {
            if (_isDisposed || added == null) return;
            _listView?.AddVariable(added);
            _listView?.Refresh();
        }

        protected virtual void OnVariableRemoved(IVariable removed)
        {
            if (_isDisposed || removed == null) return;
            _listView?.RemoveVariable(removed);
            _listView?.Refresh();
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
            _listView.Refresh();
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
