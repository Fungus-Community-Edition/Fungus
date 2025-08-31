using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;

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

                if (initArgs.Flowchart == null)
                {
                    Debug.LogError("VariableRowManager was not given a Flowchart to work with.");
                    errorLogs++;
                }

                if (initArgs.VariableListView == null)
                {
                    Debug.LogError($"VariableRowManager was not given a list view to work with.");
                    errorLogs++;
                }

                if (initArgs.HoldsManager == null)
                {
                    Debug.LogError("VariableRowManager was given nothing to hold it.");
                    errorLogs++;
                }

                if (initArgs.Root == null)
                {
                    Debug.LogError("VariableRowManager was not given a root to work with.");
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

            PrepFcEventListeners();
            void PrepFcEventListeners()
            {
                ToggleSubscriptions(false);
                _flowchart = initArgs.Flowchart;
                ToggleSubscriptions(true);
            }

            Refresh();
        }

        protected bool _isDisposed;
        protected Flowchart _flowchart;
        protected IVariableListView _listView;

        protected VisualElement _holdsManager;

        public VisualElement Root { get; protected set; }

        #region Event Wiring / Visual Init
        protected virtual void ToggleSubscriptions(bool on)
        {
            if (_flowchart == null || _listView == null)
            {
                return;
            }

            if (on)
            {
                _flowchart.VariableAdded += OnVariableAdded;
                _flowchart.VariableRemoved += OnVariableRemoved;
                _listView.OrderChanged += OnOrderChanged;
            }
            else
            {
                _flowchart.VariableAdded -= OnVariableAdded;
                _flowchart.VariableRemoved -= OnVariableRemoved;
                _listView.OrderChanged -= OnOrderChanged;
            }
        }

        protected virtual void InitVisuals(VRowManagerInitArgs initArgs)
        {
            _holdsManager = initArgs.HoldsManager;
            Root = initArgs.Root;
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

        protected virtual void OnOrderChanged(IReadOnlyList<IVariable> newlyOrderedVars)
        {
            _flowchart.ReorderVariables(newlyOrderedVars);
        }
        #endregion

        #region Refresh APIs
        /// <summary>
        /// Full rebuild: just repopulates the itemsSource list on the ListView.
        /// </summary>
        public void Refresh()
        {
            if (_isDisposed || _flowchart == null || _listView == null)
                return;

            _listView.SetVariables(_flowchart.Variables);
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

            ToggleSubscriptions(false);
            ReleaseRowsFromList();

            if (Root != null && _holdsManager != null && _holdsManager.Contains(Root))
                _holdsManager.Remove(Root);

            _listView?.Dispose();

            _listView = null;
            _flowchart = null;
            Root = null;
            _holdsManager = null;
            _isDisposed = true;
        }

        
        #endregion
    }
}
