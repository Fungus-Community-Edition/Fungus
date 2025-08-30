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

            PrepFcEventListeners();
            void PrepFcEventListeners()
            {
                ToggleSubscriptions(false);
                _flowchart = initArgs.Flowchart;
                ToggleSubscriptions(true);
            }

            InitVisuals(initArgs);

            PrepFactory();
            void PrepFactory()
            {
                if (_factory != initArgs.VariableRowFactory)
                {
                    _factory?.Dispose();
                }
                _factory = initArgs.VariableRowFactory;
            }

            PrepListView();
            void PrepListView()
            {
                if (initArgs.VariableListView != null)
                {
                    _listView = initArgs.VariableListView;
                    if (_listView is VariableListView concrete)
                    {
                        concrete.SetFactory(_factory);
                        concrete.OrderChanged += OnRowOrderChanged;
                    }
                }
                else if (_listView == null)
                {
                    Debug.LogError($"VariableRowManager was not given a list view to work with.");
                }
            }

            Refresh();
        }

        protected bool _isDisposed;
        protected Flowchart _flowchart;
        protected IVariableRowFactory _factory;
        protected IVariableListView _listView;

        protected VisualElement _holdsManager;

        public VisualElement Root { get; protected set; }

        #region Event Wiring / Visual Init
        protected virtual void ToggleSubscriptions(bool on)
        {
            if (_flowchart == null) return;

            if (on)
            {
                _flowchart.VariableAdded += OnVariableAdded;
                _flowchart.VariableRemoved += OnVariableRemoved;
            }
            else
            {
                _flowchart.VariableAdded -= OnVariableAdded;
                _flowchart.VariableRemoved -= OnVariableRemoved;
            }
        }

        protected virtual void InitVisuals(VRowManagerInitArgs initArgs)
        {
            _holdsManager = initArgs.HoldsManager;
            Root = initArgs.Root;
        }

        protected virtual void OnRowOrderChanged(IReadOnlyList<IVariable> newOrder)
        {
            if (_flowchart == null || newOrder == null) return;
            Debug.Log($"Row order changed");
            _flowchart.ReorderVariables(newOrder);
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

        #region Release Helpers
        public virtual void ReleaseRowsFromList()
        {
            // With virtualization, clearing variables triggers unbind & release logic
            _listView?.Clear();
        }
        #endregion

        #region Query
        public virtual VariableRow GetVisibleRowAt(int index) => _listView.RowAtIndex(index);
        public virtual int VisibleRowCount => _listView?.RowCount ?? 0;
        #endregion

        #region Dispose
        public virtual void Dispose()
        {
            if (_isDisposed) return;

            ToggleSubscriptions(false);
            ReleaseRowsFromList();
            if (_listView is VariableListView concrete)
                concrete.OrderChanged -= OnRowOrderChanged;

            if (Root != null && _holdsManager != null && _holdsManager.Contains(Root))
                _holdsManager.Remove(Root);

            // Factory remains owned externally; do not dispose pooled handlers unless required
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
