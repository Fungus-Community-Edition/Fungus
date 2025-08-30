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
                    // Always adopt provided view (caller controls lifecycle)
                    _listView = initArgs.VariableListView;
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

        #endregion

        #region Variable Event Handlers
        protected virtual void OnVariableAdded(IVariable added)
        {
            if (_isDisposed || added == null) return;
            CreateRowForVariable(added);
        }

        protected virtual void OnVariableRemoved(IVariable removed)
        {
            if (_isDisposed || removed == null) return;
            RemoveRowFor(removed);
        }
        #endregion

        #region Row Create / Remove
        private void CreateRowForVariable(IVariable variable)
        {
            if (variable == null || _factory == null || _listView == null) return;

            var row = _factory.Create(variable);
            _listView.AddRow(row);
        }

        protected virtual void RemoveRowFor(IVariable variable)
        {
            if (variable == null) return;

            VariableRow rowToRemove = _listView.Rows.Where((elem) => elem.VarToRepresent == variable).FirstOrDefault();

            if (rowToRemove != null)
            {
                _listView.RemoveRow(rowToRemove);
                _factory.Release(rowToRemove);
            }
            
        }
        #endregion

        #region Refresh APIs
        /// <summary>
        /// Full rebuild: releases only rows tied to the current list view, then rebuilds all rows from the Flowchart.
        /// </summary>
        public void Refresh()
        {
            if (_isDisposed || _flowchart == null || _listView == null || _factory == null)
                return;

            ReleaseRowsFromList();

            foreach (var v in _flowchart.Variables)
            {
                CreateRowForVariable(v);
            }

            _listView.Refresh();
        }

        #endregion

        #region Release Helpers
        /// <summary>
        /// Releases only rows currently parented in this manager's list view (allows prior roots to retain their visuals).
        /// </summary>
        public virtual void ReleaseRowsFromList()
        {
            if (_factory == null || _listView == null) return;

            var removalTargets = _listView.Rows.ToList();

            foreach (var elem in removalTargets)
            {
                _listView.RemoveRow(elem);
                _factory.Release(elem);
            }
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

            if (Root != null && _holdsManager != null && _holdsManager.Contains(Root))
                _holdsManager.Remove(Root);

            _factory?.Dispose();

            _factory = null;
            _listView = null;
            _flowchart = null;
            Root = null;
            _holdsManager = null;
            _isDisposed = true;
        }
        #endregion
    }
}
