using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityEditor;
using UnityEngine.Pool;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Virtualized, reorderable variable list view (Unity 2022.3 LTS + Unity 6).
    /// </summary>
    public partial class VariableListView : IVariableListView
    {
        public VariableListView(VariableListViewInitArgs initArgs)
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

        protected Flowchart _flowchart;
        protected int _flowchartInstanceID;

        // Guards & state
        bool _refreshScheduled;

        public void SetFlowchart(Flowchart flowchart)
        {
            _flowchart = flowchart;
            _flowchartInstanceID = _flowchart != null ? _flowchart.GetInstanceID() : 0;
            SyncFromFlowchart();
        }

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
                if ((uint)index >= (uint)_variables.Count)
                {
                    rowParent.userData = null;
                    rowParent.Clear();
                    return;
                }

                var variable = _variables[index];
                if (variable == null)
                {
                    rowParent.userData = null;
                    rowParent.Clear();
                    return;
                }

                rowParent.Clear();

                var row = GetOrCreateRow(variable);
                if (row?.RootElement != null)
                {
                    rowParent.Add(row.RootElement);
                }

                // Store variable to help unbind cleanup
                rowParent.userData = variable;

                // Prevent duplicate subscriptions (binding can happen many times)
                if (row != null)
                {
                    row.RemoveButtonClicked -= OnRemoveButtonClicked;
                    row.RemoveButtonClicked += OnRemoveButtonClicked;
                }

                if (_requireHandleForDrag && !string.IsNullOrEmpty(_dragHandleName) && row?.RootElement != null)
                {
                    var handle = row.RootElement.Q<VisualElement>(_dragHandleName);
                    if (handle != null && handle.userData as string != "dragHandleHooked")
                    {
                        handle.userData = "dragHandleHooked";
                        handle.RegisterCallback<PointerDownEvent>(_ => { _lastPointerDownOnHandle = true; });
                    }

                    row.RootElement.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.target != handle)
                            _lastPointerDownOnHandle = false;
                    });
                }
            };

            _listDisplay.unbindItem = (element, index) =>
            {
                // Detach per-row handlers to avoid duplicate firing after rebinding
                if (element.userData is IVariable var && _activeRows.TryGetValue(var, out var row))
                {
                    row.RemoveButtonClicked -= OnRemoveButtonClicked;
                }
                element.userData = null;
                element.Clear();
            };

            _listDisplay.destroyItem = visElem =>
            {
                visElem.userData = null;
                visElem.Clear();
            };

            _listDisplay.canStartDrag += OnCanStartDrag;
            _listDisplay.itemIndexChanged += OnItemIndexChanged;

            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
        }

        protected readonly List<IVariable> _variables = new();
        protected string _dragHandleName;
        protected bool _lastPointerDownOnHandle;

        // Schedules the actual removal to the next editor update to avoid
        // mutating ListView data source while it's mid-binding (which can cause orphan/phantom rows).
        protected virtual void OnRemoveButtonClicked(VariableRow row)
        {
            if (row == null) return;
            // Remove subscription immediately to prevent multiple queued removals
            row.RemoveButtonClicked -= OnRemoveButtonClicked;
            EditorApplication.delayCall += () => PerformRemoval(row);
        }

        void PerformRemoval(VariableRow row)
        {
            if (row == null) return;
            var variable = row.VarToRepresent;

            if (variable != null)
            {
                int idx = _variables.IndexOf(variable);
                if (idx >= 0)
                    _variables.RemoveAt(idx);

                Flowchart flowchart = _flowchart;
                if (flowchart == null && variable is Component comp)
                {
                    flowchart = comp.GetComponent<Flowchart>() ?? comp.GetComponentInParent<Flowchart>();
                }
                if (flowchart != null && _flowchart == null)
                {
                    _flowchart = flowchart;
                    _flowchartInstanceID = _flowchart.GetInstanceID();
                }

                if (!Application.isPlaying)
                {
                    int group = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Remove Variable");
                    if (flowchart != null)
                        Undo.RegisterCompleteObjectUndo(flowchart, "Remove Variable");

                    if (variable is UnityEngine.Object unityObj)
                        Undo.DestroyObjectImmediate(unityObj);

                    Undo.CollapseUndoOperations(group);
                }
                else
                {
                    if (variable is UnityEngine.Object uo)
                        UnityEngine.Object.Destroy(uo);
                }

                ReleaseRow(variable);
            }

            SafeRefresh();
        }

        void SafeRefresh()
        {
            if (_listDisplay == null) return;

            // Debounce multiple refresh requests within the same editor loop
            if (_refreshScheduled) return;
            _refreshScheduled = true;

            EditorApplication.delayCall += () =>
            {
                if (_listDisplay == null) { _refreshScheduled = false; return; }

                _refreshScheduled = false;

                // Full rebuild strategy to eliminate “phantom” rows:
                // 1. Break the binding (null itemsSource) so the internal virtualization
                //    & dynamic height caches are flushed.
                // 2. Re‑assign itemsSource.
                // 3. Release any orphaned visuals still tracked (defensive).
                // 4. Force a Rebuild (heavier than RefreshItems, but reliable after
                //    mid‑frame data mutation + component destruction).
                _listDisplay.itemsSource = null;

                // (Optional micro‑optimization: prune any rows whose variable was destroyed)
                PruneDeadRows();

                _listDisplay.itemsSource = _variables;

                // Force internal pools / height cache to recompute
                _listDisplay.Rebuild();

                // Final count & UI update
                UpdateCount();
            };
        }

        // Remove any cached row entries whose underlying variable object was destroyed (now null).
        void PruneDeadRows()
        {
            if (_activeRows.Count == 0) return;

            // Unity “missing component” slots can yield null comparison true
            // so we filter any dictionary entries whose key (IVariable) is now a UnityEngine.Object that is null.
            var dead = ListPool<IVariable>.Get();
            foreach (var kvp in _activeRows)
            {
                if (kvp.Key is UnityEngine.Object uo && uo == null)
                    dead.Add(kvp.Key);
            }

            if (dead.Count > 0)
            {
                foreach (var d in dead)
                    ReleaseRow(d);
            }
            ListPool<IVariable>.Release(dead);
        }

        protected virtual bool OnCanStartDrag(CanStartDragArgs args)
        {
            if (Application.isPlaying) return false;
            if (_requireHandleForDrag && !_lastPointerDownOnHandle)
                return false;
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
                ReleaseRow(rowElem);
            _activeRows.Clear();
        }

        public void AddVariable(IVariable variable)
        {
            if (variable == null || _variables.Contains(variable)) return;
            _variables.Add(variable);
            SafeRefresh();
        }

        public void RemoveVariable(IVariable variable)
        {
            if (variable == null) return;
            int idx = _variables.IndexOf(variable);
            if (idx < 0) return;
            _variables.RemoveAt(idx);
            ReleaseRow(variable);
            SafeRefresh();
        }

        public void SetVariables(IEnumerable<IVariable> vars)
        {
            ReleaseAllActiveRows();
            _variables.Clear();
            if (vars != null)
            {
                foreach (var elem in vars)
                    if (elem != null)
                        _variables.Add(elem);
            }
            SafeRefresh();
        }

        public void Clear()
        {
            ReleaseAllActiveRows();
            _variables.Clear();
            SafeRefresh();
        }

        public void Refresh()
        {
            SafeRefresh();
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

        public event Action<IReadOnlyList<IVariable>> OrderChanged;

        public void Dispose()
        {
            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;

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
            _countDisplay = null;
            _factory = null;

            _flowchart = null;
            _flowchartInstanceID = 0;
        }

        public void RequireDragHandle(string handleName)
        {
            _requireHandleForDrag = !string.IsNullOrEmpty(handleName);
            _dragHandleName = handleName;
        }

        protected bool _requireHandleForDrag;

        #region Undo/Redo Sync

        private void HandleUndoRedoPerformed()
        {
            AcquireFlowchartIfLost();
            SyncFromFlowchart();
        }

        protected bool AcquireFlowchartIfLost()
        {
            if (_flowchart != null) return true;

            if (_flowchartInstanceID != 0)
            {
                var obj = EditorUtility.InstanceIDToObject(_flowchartInstanceID) as Flowchart;
                if (obj != null)
                {
                    _flowchart = obj;
                    return true;
                }
            }

            try
            {
                var viaWindow = FlowchartWindow.GetFlowchart();
                if (viaWindow != null)
                {
                    _flowchart = viaWindow;
                    _flowchartInstanceID = _flowchart.GetInstanceID();
                    return true;
                }
            }
            catch { }

            var all = UnityEngine.Object.FindObjectsOfType<Flowchart>();
            if (all.Length == 1)
            {
                _flowchart = all[0];
                _flowchartInstanceID = _flowchart.GetInstanceID();
                return true;
            }

            return false;
        }

        protected virtual void SyncFromFlowchart()
        {
            if (!AcquireFlowchartIfLost())
                return;

            var source = _flowchart.Variables;
            if (source == null)
                return;

            ReleaseAllActiveRows();
            _variables.Clear();
            foreach (var elem in source)
            {
                if (elem != null)
                    _variables.Add(elem);
            }

            SafeRefresh();
        }

        #endregion

        #region For tests only
        public void ForTests_SetLastPointerDownOnHandle(bool v) => _lastPointerDownOnHandle = v;
            
        public void ForceMaterializeAllRowsForTests()
        {
            if (_listDisplay == null || _variables.Count == 0)
                return;

            var container = _listDisplay.contentContainer;
            if (container == null)
            {
                if (_testMaterializedContainer == null)
                {
                    _testMaterializedContainer = new VisualElement { name = "__TestMaterializedRows" };
                    _listDisplay.hierarchy.Add(_testMaterializedContainer);
                }
                container = _testMaterializedContainer;
            }

            if (container.childCount >= _variables.Count && _activeRows.Count >= _variables.Count)
                return;

            for (int i = 0; i < _variables.Count; i++)
            {
                var elem = _variables[i];
                if (elem == null) continue;

                var row = GetOrCreateRow(elem);
                if (row?.RootElement == null) continue;

                if (row.RootElement.parent == null)
                    container.Add(row.RootElement);
            }
        }

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