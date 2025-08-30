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
    /// FIXES:
    /// - Explicitly reorder the backing _variables list in OnItemIndexChanged (Unity does NOT automatically mutate IList).
    /// - Removed RefreshItems() inside reorder (was causing transient empty container).
    /// - Stopped releasing rows on simple unbind; only release on actual removal / clear to prevent momentary blank rows.
    /// </summary>
    public class VariableListView : IVariableListView
    {
        public VariableListView(ListView list, UITKLabel count, ILayoutRefresher refresher)
        {
            _listDisplay = list;
            _countDisplay = count;
            _refresher = refresher;
            if (_listDisplay != null)
                InitListViewStructure();
        }

        protected ListView _listDisplay;
        protected UITKLabel _countDisplay;
        protected ILayoutRefresher _refresher;
        protected IVariableRowFactory _factory;

        protected readonly List<IVariable> _variables = new();
        // Active rows kept by variable; we now retain them across unbinds to avoid flicker / empties
        protected readonly Dictionary<IVariable, VariableRow> _activeRows = new();

        bool _requireHandleForDrag;
        string _dragHandleName;
        bool _lastPointerDownOnHandle;

        public void SetFactory(IVariableRowFactory factory) => _factory = factory;

        void InitListViewStructure()
        {
            _listDisplay.itemsSource = _variables;
            _listDisplay.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _listDisplay.reorderable = true;
            _listDisplay.selectionType = SelectionType.Single;
            _listDisplay.showAlternatingRowBackgrounds = AlternatingRowBackground.All;

            _listDisplay.makeItem = () =>
            {
                var ve = new VisualElement { name = "VariableRowContainer" };
                var s = ve.style;
                s.flexDirection = FlexDirection.Column;
                s.position = Position.Relative;
                s.flexGrow = 0;
                s.flexShrink = 0;
                return ve;
            };

            _listDisplay.bindItem = (element, index) =>
            {
                if ((uint)index >= (uint)_variables.Count) return;
                var variable = _variables[index];
                if (variable == null) return;

                element.Clear();
                var row = GetOrCreateRow(variable);
                if (row?.RootElement != null)
                {
                    var rs = row.RootElement.style;
                    rs.position = Position.Relative;
                    rs.flexGrow = 0;
                    rs.flexShrink = 0;
                    rs.display = DisplayStyle.Flex;
                    element.Add(row.RootElement);
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

            _listDisplay.destroyItem = ve => ve.Clear();

            _listDisplay.canStartDrag += OnCanStartDrag;
            _listDisplay.itemIndexChanged += OnItemIndexChanged;
        }

        bool OnCanStartDrag(CanStartDragArgs args)
        {
            if (Application.isPlaying) return false;
            if (_requireHandleForDrag && !_lastPointerDownOnHandle)
                return false;
            // Reset so subsequent drags require a fresh handle click
            _lastPointerDownOnHandle = false;
            return true;
        }

        void OnItemIndexChanged(int from, int to)
        {
            if (from == to) return;
            if (_variables.Count == 0) return;

            // IMPORTANT:
            // Earlier we manually removed+inserted, assuming ListView did NOT mutate itemsSource.
            // The observed off-by-one + pair swapping indicates ListView ALREADY applied its own
            // internal reorder to _variables (because it holds a direct reference to the List<T>).
            // Our extra move then performed a second reorder, producing the wrong final order.
            //
            // Fix: Treat the backing list as already updated. Just propagate the new order.
            // If (for some future Unity version) this stops working, define AMANITA_FORCE_MANUAL_REORDER
            // and we’ll fall back to explicit move logic.

#if AMANITA_FORCE_MANUAL_REORDER
            if ((uint)from < (uint)_variables.Count && (uint)to < (uint)_variables.Count)
            {
                // Manual fallback (only enable if Unity stops mutating the list automatically).
                var item = _variables[from];
                _variables.RemoveAt(from);
                if (from < to) to -= 1;
                if (to < 0) to = 0;
                if (to > _variables.Count) to = _variables.Count;
                _variables.Insert(to, item);
            }
#endif

#if UNITY_EDITOR && AMANITA_DIAGNOSTICS
            // Diagnostics: dump current order and indexes involved.
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append($"[VariableListView] Reorder event from {from} to {to}. Current order:\n");
            for (int i = 0; i < _variables.Count; i++)
                sb.Append($"{i}: {_variables[i]?.Key}\n");
            Debug.Log(sb.ToString());
#endif

            OrderChanged?.Invoke(_variables.ToList());
            UpdateCount();
        }

        VariableRow GetOrCreateRow(IVariable variable)
        {
            if (variable == null || _factory == null) return null;
            if (_activeRows.TryGetValue(variable, out var existing)) return existing;

            var row = _factory.Create(variable);
            if (row != null)
                _activeRows[variable] = row;
            return row;
        }

        void ReleaseRow(IVariable variable)
        {
            if (variable == null) return;
            if (_activeRows.TryGetValue(variable, out var row))
            {
                _activeRows.Remove(variable);
                _factory?.Release(row);
            }
        }

        void ReleaseAllActiveRows()
        {
            if (_activeRows.Count == 0) return;
            foreach (var v in _activeRows.Keys.ToList())
                ReleaseRow(v);
            _activeRows.Clear();
        }

        // Public API
        public void AddVariable(IVariable variable)
        {
            if (variable == null || _variables.Contains(variable)) return;
            _variables.Add(variable);
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
                foreach (var v in vars)
                    if (v != null) _variables.Add(v);
            }
            _listDisplay.RefreshItems();
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
            UpdateCount();
            ScheduleLayoutFix();
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
                _countDisplay.text = $"Count: {_variables.Count}";
        }

        public void ScheduleLayoutFix() => _refresher?.Refresh(_listDisplay);

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
            _countDisplay?.RemoveFromHierarchy();
            _countDisplay = null;
            _factory = null;
            _refresher = null;
        }

        // Public API to enable drag-handle mode
        public void RequireDragHandle(string handleName)
        {
            _requireHandleForDrag = !string.IsNullOrEmpty(handleName);
            _dragHandleName = handleName;
        }

        // (optional) test helper (internal so normal builds ignore misuse)
#if UNITY_EDITOR
        public void ForTests_SetLastPointerDownOnHandle(bool v) => _lastPointerDownOnHandle = v;
#endif
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
}