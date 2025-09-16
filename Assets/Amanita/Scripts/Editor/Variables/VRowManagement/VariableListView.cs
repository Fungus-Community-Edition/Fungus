using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObj = UnityEngine.Object;

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
            _rowFactory = initArgs.RowFactory;
            if (_listDisplay != null)
                InitListViewStructure();
        }

        protected ListView _listDisplay;
        protected UITKLabel _countDisplay;
        protected IVariableRowFactory _rowFactory;

        protected Flowchart _flowchart;
        protected int _flowchartInstanceID;

        public virtual void SetFlowchart(Flowchart flowchart)
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
            _listDisplay.itemsSource = varsToDisplay;
            _listDisplay.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            _listDisplay.reorderable = true;
            _listDisplay.selectionType = SelectionType.Single;
            _listDisplay.showAlternatingRowBackgrounds = AlternatingRowBackground.All;

            PrepRowLifecycleFuncs();
            void PrepRowLifecycleFuncs()
            {
                SetMakeItem();
                void SetMakeItem()
                {
                    _listDisplay.makeItem = () =>
                    {
                        // The items made here will be the parents of the var rows'
                        // root elements
                        var rowHolder = new VisualElement { name = "VariableRowContainer" };
                        var styleForElem = rowHolder.style;
                        styleForElem.flexDirection = FlexDirection.Column;
                        styleForElem.position = Position.Relative;
                        styleForElem.flexGrow = 0;
                        styleForElem.flexShrink = 0;
                        return rowHolder;
                    };
                }

                SetBindItem();
                void SetBindItem()
                {
                    _listDisplay.bindItem = (rowHolder, index) =>
                    {
                        bool indexInRange = index < varsToDisplay.Count;
                        if (!indexInRange)
                        {
                            rowHolder.userData = null;
                            rowHolder.Clear();
                            return;
                        }

                        IVariable currentVar = varsToDisplay[index];
                        if (currentVar == null)
                        {
                            rowHolder.userData = null;
                            rowHolder.Clear();
                            return;
                        }

                        rowHolder.Clear();
                        VariableRow row = GetRowFor(currentVar);
                        VariableRow GetRowFor(IVariable variable)
                        {
                            VariableRow row = GetOrCreateRow(currentVar);
                            if (row == null)
                            {
                                Debug.LogWarning($"VariableListView.bindItem: GetOrCreateRow returned null for index={index}, key={currentVar.Key}, type={currentVar.ContentType?.FullName}");
                                rowHolder.userData = null;
                            }
                            else if (row.RootElement == null)
                            {
                                Debug.LogWarning($"VariableListView.bindItem: Row.RootElement is null for index={index}, key={currentVar.Key}, varType={currentVar.GetType().FullName}, contentType={currentVar.ContentType?.FullName}");
                                rowHolder.userData = null;
                                row = null;
                            }

                            return row;
                        }

                        if (row == null)
                        {
                            return;
                        }

                        // Keep subscriptions stable, dedupe to avoid duplicates
                        row.RemoveButtonClicked -= OnRemoveButtonClicked;
                        row.RemoveButtonClicked += OnRemoveButtonClicked;

                        // Attach visual
                        rowHolder.Add(row.RootElement);

                        // Store the row itself (not the variable) for any per-visual cleanup
                        rowHolder.userData = row;
                    };
                }

                SetUnbindItem();
                void SetUnbindItem()
                {
                    _listDisplay.unbindItem = (rowHolder, index) =>
                    {
                        // Despite how intuitive it feels, we should NOT release any rows
                        // here. Due to the lifecycle of these ListView funcs, releasing
                        // rows here can lead to empty ones getting displayed.
                        // Best leave the row-releases as responses to vars getting
                        // removed from the source list and such.
                        rowHolder.userData = null;
                        rowHolder.Clear();
                    };
                }

                SetDestroyItem();
                void SetDestroyItem()
                {
                    _listDisplay.destroyItem = rowHolder =>
                    {
                        rowHolder.userData = null;
                        rowHolder.Clear();
                    };
                }
            }

            HandleRemainingSubs();
            void HandleRemainingSubs()
            {
                _listDisplay.canStartDrag += OnCanStartDrag;
                _listDisplay.itemIndexChanged += OnItemReordered;
                Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
                Undo.undoRedoPerformed += HandleUndoRedoPerformed;
            }

            UpdateCount();
        }

        public virtual IReadOnlyList<IVariable> VarsToDisplay => varsToDisplay;

        protected readonly List<IVariable> varsToDisplay = new();
        // ^Meant to be separate from that held by the source or FC

        // Schedules the actual removal to the next editor update to avoid
        // mutating ListView data source while it's mid-binding (which can cause orphan/phantom rows).
        protected virtual void OnRemoveButtonClicked(VariableRow row)
        {
            if (!WeShouldCare())
            {
                return;
            }
            bool WeShouldCare()
            {
                if (row == null || row.VarToRepresent == null)
                {
                    return false;
                }

                var variable = row.VarToRepresent;
                int index = varsToDisplay.IndexOf(variable);
                bool isRegisteredWithUs = index >= 0;
                return isRegisteredWithUs;
            }

            IVariable variable = row.VarToRepresent;
            varsToDisplay.Remove(variable);
            Refresh();
            ReleaseRow(variable);
            // ^Why after refresh? To avoid mid-bind detach.

            DestroyUnderlyingObjectWithProperUndo();
            void DestroyUnderlyingObjectWithProperUndo()
            {
                if (!Application.isPlaying)
                {
                    int group = Undo.GetCurrentGroup();
                    string contentTypeName = variable.ContentType.Name;
                    string groupName = $"Remove {contentTypeName} Variable";
                    Undo.SetCurrentGroupName(groupName);

                    if (_flowchart != null)
                        Undo.RegisterCompleteObjectUndo(_flowchart, groupName);

                    // The legacy vars are MonoBehaviours, so...
                    if (variable is UnityObj legacyVar && legacyVar != null)
                        Undo.DestroyObjectImmediate(legacyVar);

                    Undo.CollapseUndoOperations(group);
                }
                else
                {
                    if (variable is UnityObj legacyVar && legacyVar != null)
                        UnityObj.Destroy(legacyVar);
                }
            }
        }

        protected virtual bool OnCanStartDrag(CanStartDragArgs args)
        {
            if (Application.isPlaying) return false;
            return true;
        }

        protected virtual void OnItemReordered(int from, int to)
        {
            if (from == to) return;
            if (varsToDisplay.Count == 0) return;
            OrderChanged?.Invoke(varsToDisplay);
        }

        protected GlobalObjectId _flowchartGlobalId;

        protected virtual VariableRow GetOrCreateRow(IVariable variable)
        {
            if (variable == null || _rowFactory == null) return null;

            bool rowAlreadyAssignedToIt = _activeRows.TryGetValue(variable, out var existing);
            if (rowAlreadyAssignedToIt)
            {
                return existing;
            }

            var row = _rowFactory.Create(variable);
            if (row != null)
            {
                _activeRows[variable] = row;
            }

            return row;
        }

        protected readonly Dictionary<IVariable, VariableRow> _activeRows = new();

        protected virtual void ReleaseRow(IVariable variable)
        {
            if (variable == null) return;
            if (_activeRows.TryGetValue(variable, out var row))
            {
                _activeRows.Remove(variable);
                _rowFactory?.Release(row);
            }
        }

        protected virtual void ReleaseAllActiveRows()
        {
            if (_activeRows.Count == 0) return;
            foreach (var rowElem in _activeRows.Keys.ToList())
                ReleaseRow(rowElem);
            _activeRows.Clear();
        }

        public virtual void AddVariable(IVariable toAdd)
        {
            if (this._isDisposed)
            {
                string logMessage = $"Tried to add variable to disposed VariableListView.";
                Debug.LogWarning(logMessage);
                return;
            }
            if (toAdd == null || varsToDisplay.ContainsReference(toAdd))
            {
                string logMessage = $"Tried to add a null variable to VariableListView.";
                Debug.LogWarning(logMessage);
                return;
            }

            varsToDisplay.Add(toAdd);
            Refresh();
        }

        public virtual void RemoveVariable(IVariable variable)
        {
            if (variable == null) return;
            int idx = varsToDisplay.IndexOf(variable);
            if (idx < 0) return;

            varsToDisplay.RemoveAt(idx);
            ReleaseRow(variable);
            Refresh();
        }

        public virtual void SetVariables(IEnumerable<IVariable> vars)
        {
            ReleaseAllActiveRows();
            varsToDisplay.Clear();
            if (vars != null)
            {
                foreach (var elem in vars)
                    if (elem != null)
                        varsToDisplay.Add(elem);
            }
            Refresh();
        }

        public virtual void Clear()
        {
            ReleaseAllActiveRows();
            varsToDisplay.Clear();
            Refresh();
        }

        public virtual void Refresh()
        {
            _listDisplay.RefreshItems();
            UpdateCount();
        }

        public int RowCount => varsToDisplay.Count;
        public IReadOnlyList<VariableRow> Rows => _activeRows.Values.ToList();

        public VariableRow RowAtIndex(int index)
        {
            if ((uint)index >= (uint)varsToDisplay.Count) return null;
            var v = varsToDisplay[index];
            _activeRows.TryGetValue(v, out var row);
            return row;
        }

        public bool Contains(VariableRow row) => row != null && _activeRows.Values.Contains(row);

        public virtual void UpdateCount()
        {
            if (_countDisplay != null)
                _countDisplay.text = $"Count: {varsToDisplay.Count}";
        }

        public event Action<IList<IVariable>> OrderChanged;

        public virtual void Dispose()
        {
            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;

            ReleaseAllActiveRows();
            varsToDisplay.Clear();

            if (_listDisplay != null)
            {
                _listDisplay.makeItem = null;
                _listDisplay.bindItem = null;
                _listDisplay.unbindItem = null;
                _listDisplay.destroyItem = null;
                _listDisplay.itemIndexChanged -= OnItemReordered;
                _listDisplay.canStartDrag -= OnCanStartDrag;
                _listDisplay.Clear();
                _listDisplay = null;
            }

            _listDisplay?.RemoveFromHierarchy();
            _countDisplay?.RemoveFromHierarchy();
            _countDisplay = null;
            _rowFactory = null;

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
            IList<Flowchart> all;

#if UNITY_6000_0_OR_NEWER
            all = UnityObj.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
#else
            all = UnityObj.FindObjectsOfType<Flowchart>();
#endif

            if (all.Count == 1)
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
            varsToDisplay.Clear();
            var sourceToAdd = source.Where(IsValidVar);

            static bool IsValidVar(IVariable elem)
            {
                // As in neither null or a destroyed UnityObj
                return elem != null && (elem is not UnityObj uo || uo != null);
            }

            varsToDisplay.AddRange(sourceToAdd);
            Refresh();
        }

        #endregion

        #region For tests only
        public virtual void ForceMaterializeAllRowsForTests()
        {
            if (_listDisplay == null || varsToDisplay.Count == 0)
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

            if (container.childCount >= varsToDisplay.Count && _activeRows.Count >= varsToDisplay.Count)
                return;

            for (int i = 0; i < varsToDisplay.Count; i++)
            {
                var elem = varsToDisplay[i];
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
        event Action<IList<IVariable>> OrderChanged;
    }

    public class VariableListViewInitArgs
    {
        public IVariableRowFactory RowFactory { get; set; }
        public ListView List { get; set; }
        public UITKLabel CountLabel { get; set; }
    }
}