using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObj = UnityEngine.Object;
using AtMycelia.Collections;
using AtMycelia.EditorUtils;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Virtualized, reorderable variable list view (Unity 2022.3 LTS + Unity 6).
    /// Handles making sure that the VariableRows are shown correctly.
    /// </summary>
    public partial class VariableListView : IVariableListView
    {
        public VariableListView(VariableListViewInitArgs initArgs)
        {
            _listDisplay = initArgs.List;
            _countDisplay = initArgs.CountLabel;
            _rowFactory = initArgs.RowFactory;
            _variableSourceContext = initArgs.VariableSource as UnityObj;

            if (_variableSourceContext == null)
            {
                Debug.LogWarning($"VariableListView was not given a valid variable source context" +
                    $" (Flowchart or VariableSourceAsset). Some operations may not work as intended.");
            }
            else
            {
                SetVariables(initArgs.VariableSource.Variables);
                _lastSourceUid = initArgs.VariableSource.UniqueId;
            }
            if (_listDisplay != null)
            {
                InitListViewStructure();
            }
            else
            {
                string errorMessage = $"VariableListView was not given a valid ListView in its init args.";
                Debug.LogError(errorMessage);
            }

        }

        protected ListView _listDisplay;
        protected UITKLabel _countDisplay;
        protected IVariableRowFactory _rowFactory;

        protected UnityObj _variableSourceContext;

        public virtual void SetSource(IVariableSource source)
        {
            _source = source;
            _lastSourceUid = _source != null ? _source.UniqueId : string.Empty;
            SyncFromSource();
        }

        private IVariableSource _source;

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
                        // The items made here will be the parents of the var row handlers'
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
                        rowHolder.userData = null;
                        VariableRow row = GetRowFor(currentVar);
                        VariableRow GetRowFor(IVariable variable)
                        {
                            VariableRow row = GetOrCreateRow(currentVar);
                            if (row == null)
                            {
                                Debug.LogError($"VariableListView.bindItem: GetOrCreateRow returned null for index={index}, key={currentVar.Key}, type={currentVar.ContentType?.FullName}");
                            }
                            else if (row.RootElement == null)
                            {
                                Debug.LogError($"VariableListView.bindItem: Row.RootElement is null for index={index}, key={currentVar.Key}, varType={currentVar.GetType().FullName}, contentType={currentVar.ContentType?.FullName}");
                                row = null;
                            }

                            return row;
                        }

                        if (row == null)
                        {
                            return;
                        }

                        var visHandler = row.VisualHandler;

                        AttachVisual();
                        void AttachVisual()
                        {
                            // Attach visual
                            try
                            {
                                rowHolder.Add(row.RootElement);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"[VListView.bindItem] index={index} key='{currentVar.Key}' failed to Add(row.RootElement): {ex.Message}");
                            }
                        }
                        if (currentVar is Muscariable && currentVar.Owner is Flowchart)
                        {
                            visHandler.Variable = currentVar; // Just in case
                            visHandler.Refresh();
                        }
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
                        // Diagnostics: log unbind
                        try
                        {
                            if (rowHolder.userData is VariableRow r && r.VarToRepresent != null)
                            {
                                //Debug.Log($"[VListView.unbindItem] index={index} key='{r.VarToRepresent.Key}'");
                            }
                        }
                        catch { }
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

        protected virtual bool OnCanStartDrag(CanStartDragArgs args)
        {
            if (Application.isPlaying) return false;
            return true;
        }

        protected virtual void OnItemReordered(int from, int to)
        {
            if (from == to || varsToDisplay.Count == 0) return;
            OrderChanged?.Invoke(varsToDisplay);
        }

        protected virtual VariableRow GetOrCreateRow(IVariable variable)
        {
            if (variable == null || _rowFactory == null) return null;

            bool rowAlreadyAssignedToIt = _activeRows.TryGetValue(variable, out var existing);
            if (rowAlreadyAssignedToIt)
            {
                //Debug.Log($"[GetOrCreateRow] Reusing existing row for key='{variable.Key}' varHash={RuntimeHelpers.GetHashCode(variable)}");
                return existing;
            }

            var row = _rowFactory.Create(variable);
            if (row != null)
            {
                //Debug.Log($"[GetOrCreateRow] Created new row for key='{variable.Key}' varHash={RuntimeHelpers.GetHashCode(variable)} handlerType={row.VisualHandler?.GetType().FullName}");
                _activeRows[variable] = row;
            }
            else
            {
                Debug.LogWarning($"[GetOrCreateRow] Factory returned null row for key='{variable.Key}'");
            }

            return row;
        }

        // Use reference equality for keys to avoid value-based equality collisions that can
        // cause distinct variable instances to be treated as the same key.
        protected readonly Dictionary<IVariable, VariableRow> _activeRows =
            new Dictionary<IVariable, VariableRow>(new ReferenceEqualityComparer<IVariable>());

        protected virtual void ReleaseRow(IVariable variable)
        {
            if (variable == null) return;
            if (_activeRows.TryGetValue(variable, out var row))
            {
                //Debug.Log($"[ReleaseRow] Releasing row for key='{variable.Key}' varHash={RuntimeHelpers.GetHashCode(variable)}");
                _activeRows.Remove(variable);
                _rowFactory?.Release(row);
            }
        }

        protected virtual void ReleaseAllActiveRows()
        {
            if (_activeRows.Count == 0) return;
            ////Debug.Log($"[ReleaseAllActiveRows] Releasing {_activeRows.Count} active rows");
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

        public virtual void SetVariables(IEnumerable<IVariable> varsToSet)
        {
            ReleaseAllActiveRows();
            varsToDisplay.Clear();
            if (varsToSet != null)
            {
                foreach (var elem in varsToSet)
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

        IReadOnlyList<IVariable> IVariableListView.VarsToDisplay => VarsToDisplay;

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
            {
                _countDisplay.text = $"Count: {varsToDisplay.Count}";
            }
        }

        public event Action<IList<IVariable>> OrderChanged;

        public virtual void Dispose()
        {
            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;

            ReleaseAllActiveRows();
            varsToDisplay.Clear();

            DisposeListDisplay();
            void DisposeListDisplay()
            {
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
            }

            ResetCountDisplay();
            _rowFactory = null;

            _source = null;
            _isDisposed = true;

            void ResetCountDisplay()
            {
                if (_countDisplay == null)
                {
                    return;
                }

                _countDisplay.text = "Count: 0";
                _countDisplay = null;
            }
        }

        protected bool _isDisposed;


        #region Undo/Redo Sync

        protected void HandleUndoRedoPerformed()
        {
            if (!AcquireSourceIfLost())
            {
                return;
            }

            SyncFromSource();
            UpdateCount();
        }

        // Returns true if the flowchart was found (or not even lost in the first place),
        // false otherwise.
        protected bool AcquireSourceIfLost()
        {
            if (_source != null) return true;

            FindFlowchart(out bool found);
            void FindFlowchart(out bool found)
            {
                found = false;
                var fcFound = UnityObj.FindObjectsByType<Flowchart>(FindObjectsSortMode.None)
                    .Where((fc) => fc.UniqueId == _lastSourceUid).FirstOrDefault();
                if (fcFound != null)
                {
                    SetSource(fcFound);
                    found = true;
                }
            }

            if (found)
            {
                return true;
            }

            FindVsa(out found);
            void FindVsa(out bool found)
            {
                found = false;
                var vsasInProject = Resources.LoadAll<VariableSourceAsset>("");
                var vsaFound = vsasInProject.Where((vsa) => vsa.UniqueId == _lastSourceUid).FirstOrDefault();
                if (vsaFound != null)
                {
                    SetVariables(vsaFound.Variables);
                    found = true;
                }
            }
            return found;
        }

        protected virtual void SyncFromSource()
        {
            var source = _source?.Variables;
            if (source == null)
                return;

            ReleaseAllActiveRows();
            varsToDisplay.Clear();
            var sourceToAdd = source.Where(IsValidVar);

            static bool IsValidVar(IVariable elem)
            {
                // As in neither null or a destroyed UnityObj
                return elem != null && (elem is not UnityObj unityObj || unityObj != null);
            }

            varsToDisplay.AddRange(sourceToAdd);
            Refresh();
        }

        private string _lastSourceUid = string.Empty;

        #endregion

        #region For tests only
        public virtual void ForceMaterializeAllRowsForTests()
        {
            if (_listDisplay == null || varsToDisplay.Count == 0)
                return;

            VisualElement container;
            EnsureWeHaveContainer();
            void EnsureWeHaveContainer()
            {
                container = _listDisplay.contentContainer;
                if (container == null)
                {
                    if (_testMaterializedContainer == null)
                    {
                        _testMaterializedContainer = new VisualElement { name = "__TestMaterializedRows" };
                        _listDisplay.hierarchy.Add(_testMaterializedContainer);
                        Debug.Log($"[ForceMaterializeAllRowsForTests] Using test materialized container");
                    }
                    container = _testMaterializedContainer;
                }
            }

            // Removed the early bailout that skipped creation when child counts matched.
            // Tests need full row materialization regardless of existing placeholder children.
            PrepRowsForTheVars();
            void PrepRowsForTheVars()
            {
                for (int i = 0; i < varsToDisplay.Count; i++)
                {
                    var variable = varsToDisplay[i];
                    if (variable == null)
                        continue;

                    var row = GetOrCreateRow(variable);
                    if (row == null)
                        continue;

                    // If visuals not yet built, attempt a refresh to force template cloning.
                    if (row.RootElement == null && row.VisualHandler != null)
                    {
                        try
                        {
                            row.VisualHandler.Refresh();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[ForceMaterializeAllRowsForTests] Refresh failed for var key='{variable.Key}': {ex.Message}");
                        }
                    }

                    if (row.RootElement == null)
                    {
                        Debug.LogWarning($"[ForceMaterializeAllRowsForTests] Row.RootElement still null for key='{variable.Key}'. Pooling will work but UI won’t show.");
                        continue;
                    }

                    if (row.RootElement.parent == null)
                    {
                        container.Add(row.RootElement);
                    }
                }
            }
        }

        VisualElement _testMaterializedContainer;
        #endregion

    }

    /// <summary>
    /// Simple reference-equality comparer used for dictionaries that must use object identity
    /// rather than value-based equality.
    /// </summary>
    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj)
        {
            if (obj == null) return 0;
            return RuntimeHelpers.GetHashCode(obj);
        }
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
        IReadOnlyList<IVariable> VarsToDisplay { get; }
    }

    public class VariableListViewInitArgs
    {
        public IVariableRowFactory RowFactory { get; set; }
        public ListView List { get; set; }
        public UITKLabel CountLabel { get; set; }
        public IVariableSource VariableSource { get; set; }
        public IEditorAssetResolver AssetResolver { get; set; } // optional; testing override
        public bool DisplayInInspector { get; set; } = false;
    }
}