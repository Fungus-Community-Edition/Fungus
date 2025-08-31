using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Virtualized, reorderable variable list view (Unity 2022.3 LTS + Unity 6).
    /// </summary>
    public partial class VariableListView : IVariableListView
    {
        public VariableListView(VariableListViewInitArgs  initArgs)
        {
            _listDisplay = initArgs.List;
            _countDisplay = initArgs.CountLabel;
            _factory = initArgs.RowFactory;
            if (_listDisplay != null)
                InitListViewStructure();
        }

        protected ListView _listDisplay;
        protected UITKLabel _countDisplay;
        protected IVariableRowFactory _factory;

        protected virtual void InitListViewStructure()
        {
            _listDisplay.itemsSource = _variables;
            _listDisplay.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _listDisplay.reorderable = true;
            _listDisplay.selectionType = SelectionType.Single;
            _listDisplay.showAlternatingRowBackgrounds = AlternatingRowBackground.All;

            _listDisplay.makeItem = () =>
            {
                var visElem = new VisualElement { name = "VariableRowContainer" };
                var styleForElem = visElem.style;
                styleForElem.flexDirection = FlexDirection.Column;
                styleForElem.position = Position.Relative;
                styleForElem.flexGrow = 0;
                styleForElem.flexShrink = 0;
                return visElem;
            };

            _listDisplay.bindItem = (rowParent, index) =>
            {
                if ((uint)index >= (uint)_variables.Count) return;
                var variable = _variables[index];
                if (variable == null) return;

                rowParent.Clear(); // We don't want a single element to have multiple rows parented to it
                var row = GetOrCreateRow(variable);
                if (row?.RootElement != null)
                {
                    var rowStyle = row.RootElement.style;
                    rowStyle.position = Position.Relative;

                    rowStyle.flexGrow = 0;
                    rowStyle.flexShrink = 0;
                    // ^We want the size to be consistent, no matter how much the window itself 
                    // stretches

                    rowStyle.display = DisplayStyle.Flex;
                    rowParent.Add(row.RootElement);
                }

                if (_requireHandleForDrag && !string.IsNullOrEmpty(_dragHandleName))
                {
                    // Register once per bound instance (handlers are cheap)
                    var handle = row.RootElement.Q<VisualElement>(_dragHandleName);
                    if (handle != null && handle.userData as string != "dragHandleHooked")
                    {
                        handle.userData = "dragHandleHooked";
                        handle.RegisterCallback<PointerDownEvent>(_ =>
                        {
                            _lastPointerDownOnHandle = true;
                        });
                    }

                    // Row root fallback: pointer downs not on handle clear eligibility
                    row.RootElement.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.target != handle)
                            _lastPointerDownOnHandle = false;
                    });
                }
            };

            _listDisplay.unbindItem = (element, index) =>
            {
                // DO NOT release here; virtualization reuses these on reorder scroll.
                element.Clear();
            };

            _listDisplay.destroyItem = visElem => visElem.Clear();

            _listDisplay.canStartDrag += OnCanStartDrag;
            _listDisplay.itemIndexChanged += OnItemIndexChanged;
        }

        protected readonly List<IVariable> _variables = new();
        protected string _dragHandleName;
        protected bool _lastPointerDownOnHandle;

        protected virtual bool OnCanStartDrag(CanStartDragArgs args)
        {
            if (Application.isPlaying) return false;
            if (_requireHandleForDrag && !_lastPointerDownOnHandle)
                return false;
            // Reset so subsequent drags require a fresh handle click
            _lastPointerDownOnHandle = false;
            return true;
        }

        protected virtual void OnItemIndexChanged(int from, int to)
        {
            if (from == to) return;
            if (_variables.Count == 0) return;

            OrderChanged?.Invoke(_variables.ToList());
            UpdateCount();
        }

        protected virtual VariableRow GetOrCreateRow(IVariable variable)
        {
            if (variable == null || _factory == null) return null;
            if (_activeRows.TryGetValue(variable, out var existing)) return existing;

            var row = _factory.Create(variable);
            if (row != null)
                _activeRows[variable] = row;
            return row;
        }

        // Active rows kept by variable; we now retain them across unbinds to avoid flicker / empties
        protected readonly Dictionary<IVariable, VariableRow> _activeRows = new();

        protected virtual void ReleaseRow(IVariable variable)
        {
            if (variable == null) return;
            if (_activeRows.TryGetValue(variable, out var row))
            {
                _activeRows.Remove(variable);
                _factory?.Release(row);
            }
        }

        protected virtual void ReleaseAllActiveRows()
        {
            if (_activeRows.Count == 0) return;

            foreach (var rowElem in _activeRows.Keys.ToList())
            {
                ReleaseRow(rowElem);
            }

            _activeRows.Clear();
        }

        public void AddVariable(IVariable variable)
        {
            if (variable == null || _variables.Contains(variable)) return;
            _variables.Add(variable);
            // ^This list is the source for the list display, and thus adding to it and then
            // refreshing the list display should get the appropriate row added
            _listDisplay.RefreshItems();
            UpdateCount();
        }

        public void RemoveVariable(IVariable variable)
        {
            if (variable == null) return;
            int idx = _variables.IndexOf(variable);
            if (idx < 0) return;

            _variables.RemoveAt(idx);
            ReleaseRow(variable);
            _listDisplay.RefreshItems();
            UpdateCount();
        }

        public void SetVariables(IEnumerable<IVariable> vars)
        {
            ReleaseAllActiveRows();
            _variables.Clear();
            if (vars != null)
            {
                foreach (var elem in vars)
                {
                    if (elem != null)
                    {
                        _variables.Add(elem);
                    }
                }
            }

            UpdateCount();
        }

        public void Clear()
        {
            ReleaseAllActiveRows();
            _variables.Clear();
            _listDisplay.RefreshItems();
            UpdateCount();
        }

        public void Refresh()
        {
            _listDisplay.RefreshItems();
            UpdateCount();
        }

        public int RowCount => _variables.Count;
        public IReadOnlyList<VariableRow> Rows => _activeRows.Values.ToList();

        public VariableRow RowAtIndex(int index)
        {
            if ((uint)index >= (uint)_variables.Count) return null;
            var v = _variables[index];
            _activeRows.TryGetValue(v, out var row);
            return row;
        }

        public bool Contains(VariableRow row) => row != null && _activeRows.Values.Contains(row);

        public void UpdateCount()
        {
            if (_countDisplay != null)
            {
                _countDisplay.text = $"Count: {_variables.Count}";
            }
        }

        public event Action<IReadOnlyList<IVariable>> OrderChanged;

        public void Dispose()
        {
            ReleaseAllActiveRows();
            _variables.Clear();
            if (_listDisplay != null)
            {
                _listDisplay.makeItem = null;
                _listDisplay.bindItem = null;
                _listDisplay.unbindItem = null;
                _listDisplay.destroyItem = null;
                _listDisplay.itemIndexChanged -= OnItemIndexChanged;
                _listDisplay.canStartDrag -= OnCanStartDrag;
                _listDisplay.Clear();
                _listDisplay = null;
            }
            _listDisplay?.RemoveFromHierarchy();
            _countDisplay?.RemoveFromHierarchy();
            _listDisplay = null;
            _countDisplay = null;
            _factory = null;
        }

        public void RequireDragHandle(string handleName)
        {
            _requireHandleForDrag = !string.IsNullOrEmpty(handleName);
            _dragHandleName = handleName;
        }

        protected bool _requireHandleForDrag;

        #region For tests only
        // (optional) test helper (internal so normal builds ignore misuse)
        public void ForTests_SetLastPointerDownOnHandle(bool v) => _lastPointerDownOnHandle = v;

        // Force-creates rows & handlers for all current variables
        // without requiring a panel / real binding cycle.
        // Safely handles the case where ListView's internal scroll view (and thus contentContainer)
        // has not been created yet (with contentContainer == null).
        public void ForceMaterializeAllRowsForTests()
        {
            if (_listDisplay == null || _variables.Count == 0)
                return;

            // If ListView hasn't created its internal ScrollView yet, contentContainer will be null.
            // We create (once) a private fallback container to host materialized rows for tests.
            var container = _listDisplay.contentContainer;
            if (container == null)
            {
                if (_testMaterializedContainer == null)
                {
                    _testMaterializedContainer = new VisualElement
                    {
                        name = "__TestMaterializedRows"
                    };
                    // Add it directly under the ListView so tests can still inspect visual children if needed.
                    _listDisplay.hierarchy.Add(_testMaterializedContainer);
                }
                container = _testMaterializedContainer;
            }

            // Avoid redundant population: if we already have as many root row visuals
            // as variables, assume it's up to date (headless test scenario).
            if (container.childCount >= _variables.Count && _activeRows.Count >= _variables.Count)
                return;

            for (int i = 0; i < _variables.Count; i++)
            {
                var elem = _variables[i];
                if (elem == null)
                    continue;

                var row = GetOrCreateRow(elem);
                if (row?.RootElement == null)
                    continue;

                if (row.RootElement.parent == null)
                    container.Add(row.RootElement);
            }
        }

        // Fallback container for headless test materialization (never serialized / runtime only).
        VisualElement _testMaterializedContainer;
        #endregion
    }

    public interface IVariableListView : IDisposable
    {
        void AddVariable(IVariable variable);
        void RemoveVariable(IVariable variable);
        void SetVariables(IEnumerable<IVariable> variables);
        void Clear();
        VariableRow RowAtIndex(int index);
        int RowCount { get; }
        void Refresh();
        IReadOnlyList<VariableRow> Rows { get; }
        bool Contains(VariableRow row);
        event Action<IReadOnlyList<IVariable>> OrderChanged;
    }

    public class VariableListViewInitArgs
    {
        public IVariableRowFactory RowFactory { get; set; }
        public ListView List { get; set; }
        public UITKLabel CountLabel { get; set; }
    }
}