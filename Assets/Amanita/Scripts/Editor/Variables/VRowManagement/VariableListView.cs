using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        public void SetFlowchart(Flowchart flowchart)
        {
            _flowchart = flowchart;
            if (_flowchart != null)
            {
                _flowchartInstanceID = _flowchart.GetInstanceID();
                _flowchartGlobalId = GlobalObjectId.GetGlobalObjectIdSlow(_flowchart);
            }
            else
            {
                _flowchartInstanceID = 0;
                _flowchartGlobalId = default;
            }
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
                if (row == null || row.RootElement == null)
                {
                    rowParent.userData = null;
                    return;
                }

                // Keep subscriptions stable, dedupe to avoid duplicates
                row.RemoveButtonClicked -= OnRemoveButtonClicked;
                row.RemoveButtonClicked += OnRemoveButtonClicked;

                // Attach visual
                rowParent.Add(row.RootElement);

                // Store the row itself (not the variable) for any per-visual cleanup
                rowParent.userData = row;


            };

            _listDisplay.unbindItem = (element, index) =>
            {
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

            UpdateCount();
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
            if (row == null || row.VarToRepresent == null) return;

            var variable = row.VarToRepresent;
            string contentTypeName = variable.ContentType.Name;

            int idx = _variables.IndexOf(variable);
            if (idx < 0) return;

            _variables.RemoveAt(idx);

            Refresh();

            // 3) Release only the removed row (after refresh to avoid mid-bind detach)
            ReleaseRow(variable);

            // 4) Destroy underlying object with proper Undo
            if (!Application.isPlaying)
            {
                int group = Undo.GetCurrentGroup();
                string groupName = $"Remove {contentTypeName} Variable";
                Undo.SetCurrentGroupName(groupName);

                if (_flowchart != null)
                    Undo.RegisterCompleteObjectUndo(_flowchart, groupName);

                if (variable is UnityEngine.Object unityObj && unityObj != null)
                    Undo.DestroyObjectImmediate(unityObj);

                Undo.CollapseUndoOperations(group);
            }
            else
            {
                if (variable is UnityEngine.Object uo && uo != null)
                    UnityEngine.Object.Destroy(uo);
            }
        }

        protected virtual bool OnCanStartDrag(CanStartDragArgs args)
        {
            if (Application.isPlaying) return false;
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

        protected GlobalObjectId _flowchartGlobalId;


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
            if (this._isDisposed)
            {
                string logMessage = $"Tried to add variable to disposed VariableListView.";
                Debug.LogWarning(logMessage);
                return;
            }
            if (variable == null || _variables.Contains(variable))
            {
                string logMessage = $"Tried to add a null variable to VariableListView.";
                Debug.LogWarning(logMessage);
                return;
            }

            _variables.Add(variable);
            Refresh();
        }

        public void RemoveVariable(IVariable variable)
        {
            if (variable == null) return;
            int idx = _variables.IndexOf(variable);
            if (idx < 0) return;

            _variables.RemoveAt(idx);
            ReleaseRow(variable);
            Refresh();
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
            Refresh();
        }

        public void Clear()
        {
            ReleaseAllActiveRows();
            _variables.Clear();
            Refresh();
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
            _isDisposed = true;
        }

        protected bool _isDisposed;


        #region Undo/Redo Sync

        private void HandleUndoRedoPerformed()
        {
            AcquireFlowchartIfLost();
            SyncFromFlowchart();
            UpdateCount();
        }

        protected bool AcquireFlowchartIfLost()
        {
            if (_flowchart != null) return true;

            // 1) Try GlobalObjectId first
            if (_flowchartGlobalId.identifierType != 0) // default struct check
            {
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_flowchartGlobalId) as Flowchart;
                if (obj != null)
                {
                    SetFlowchart(obj);
                    return true;
                }
            }

            // 2) Fallback to old instance ID (may fail after undo/redo)
            if (_flowchartInstanceID != 0)
            {
                var obj = EditorUtility.InstanceIDToObject(_flowchartInstanceID) as Flowchart;
                if (obj != null)
                {
                    SetFlowchart(obj);
                    return true;
                }
            }

            // 3) Try FlowchartWindow
            try
            {
                var viaWindow = FlowchartWindow.GetFlowchart();
                if (viaWindow != null)
                {
                    SetFlowchart(viaWindow);
                    return true;
                }
            }
            catch { }

            // 4) Fallback: single Flowchart in scene
            var all = UnityEngine.Object.FindObjectsOfType<Flowchart>();
            if (all.Length == 1)
            {
                SetFlowchart(all[0]);
                return true;
            }

            return false;
        }

        protected virtual void SyncFromFlowchart()
        {
            var source = _flowchart.Variables;
            if (source == null)
                return;

            ReleaseAllActiveRows();
            _variables.Clear();
            var sourceToAdd = source.Where(IsValidVar);

            bool IsValidVar(IVariable elem)
            {
                // As in neither null or a destroyed UnityObj
                return elem != null && (!(elem is UnityEngine.Object uo) || uo != null);
            }

            _variables.AddRange(sourceToAdd);
            Refresh();
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