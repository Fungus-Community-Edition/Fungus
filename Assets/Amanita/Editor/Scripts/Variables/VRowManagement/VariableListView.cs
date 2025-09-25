using Amanita.EditorUtils;
using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
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

            // Resolver: prefer explicit injection, fallback to the global maintenance resolver.
            _assetResolver = initArgs.AssetResolver ?? VariableSourceAssetMaintenance.AssetResolver;

            // Rebind active rows after domain/assembly reloads so SerializedObjects (holders/assets)
            // are refreshed and UI fields don't appear empty.
            AssemblyReloadEvents.afterAssemblyReload -= HandleAfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += HandleAfterAssemblyReload;

            if (_variableSourceContext == null)
            {
                Debug.LogWarning($"VariableListView was not given a valid variable source context" +
                    $" (Flowchart or VariableSourceAsset). Some operations may not work as intended.");
            }
            else
            {
                SetVariables(initArgs.VariableSource.Variables);
            }
            if (_listDisplay != null)
                InitListViewStructure();
        }

        protected ListView _listDisplay;
        protected UITKLabel _countDisplay;
        protected IVariableRowFactory _rowFactory;

        protected Flowchart _flowchart;
        protected int _flowchartInstanceID;
        protected UnityObj _variableSourceContext;

        // New: resolver used for asset lookups so editor code can be tested/mocked.
        protected IEditorAssetResolver _assetResolver;

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

                        // Diagnostics: log bind attempt
                        try
                        {
                            Debug.Log($"[VListView.bindItem] index={index} key='{currentVar.Key}' varType={currentVar.GetType().FullName} varHash={RuntimeHelpers.GetHashCode(currentVar)}");
                        }
                        catch { }

                        rowHolder.Clear();
                        rowHolder.userData = null;
                        VariableRow row = GetRowFor(currentVar);
                        VariableRow GetRowFor(IVariable variable)
                        {
                            VariableRow row = GetOrCreateRow(currentVar);
                            if (row == null)
                            {
                                Debug.LogWarning($"VariableListView.bindItem: GetOrCreateRow returned null for index={index}, key={currentVar.Key}, type={currentVar.ContentType?.FullName}");
                            }
                            else if (row.RootElement == null)
                            {
                                Debug.LogWarning($"VariableListView.bindItem: Row.RootElement is null for index={index}, key={currentVar.Key}, varType={currentVar.GetType().FullName}, contentType={currentVar.ContentType?.FullName}");
                                row = null;
                            }

                            return row;
                        }

                        if (row == null)
                        {
                            return;
                        }

                        // **Resolve the correct binding target**
                        var targetObj = GetBindingTarget(currentVar);

                        // Diagnostics: log target resolution
                        if (targetObj == null)
                        {
                            Debug.Log($"[VListView.bindItem] index={index} key='{currentVar.Key}' -> targetObj: null");
                        }
                        else
                        {
                            string path = null;
                            try
                            {
                                path = _assetResolver?.GetAssetPath(targetObj);
                            }
                            catch { }
                            Debug.Log($"[VListView.bindItem] index={index} key='{currentVar.Key}' -> targetObj: type={targetObj.GetType().FullName} name='{targetObj.name}' instanceId={targetObj.GetInstanceID()} path='{path}'");
                        }

                        // **Inject the SerializedObject into the already-initialized row**
                        if (targetObj != null)
                        {
                            try
                            {
                                var so = new SerializedObject(targetObj);
                                row.VisualHandler.SerializedVar = so;
                                Debug.Log($"[VListView.bindItem] index={index} key='{currentVar.Key}' Assigned SerializedObject targeting instanceId={targetObj.GetInstanceID()}");
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[VListView.bindItem] index={index} key='{currentVar.Key}' Failed to create SerializedObject: {ex.Message}");
                            }
                        }

                        // Attach visual
                        try
                        {
                            rowHolder.Add(row.RootElement);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[VListView.bindItem] index={index} key='{currentVar.Key}' failed to Add(row.RootElement): {ex.Message}");
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
                                Debug.Log($"[VListView.unbindItem] index={index} key='{r.VarToRepresent.Key}'");
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
                        // Diagnostics: log destroy
                        try
                        {
                            if (rowHolder.userData is VariableRow r && r.VarToRepresent != null)
                                Debug.Log($"[VListView.destroyItem] destroying row for key='{r.VarToRepresent.Key}'");
                        }
                        catch { }
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

        protected virtual UnityObj GetBindingTarget(IVariable variable)
        {
            if (variable is UnityObj unityObj)
                return unityObj; // Legacy variable

            return FindPersistentHolderFor(variable, _variableSourceContext);
        }

        protected virtual MuscariableHolder FindPersistentHolderFor(IVariable variable, UnityObj context)
        {
            if (context == null || _assetResolver == null) return null;

            var path = _assetResolver.GetAssetPath(context);

            // Use resolver to enumerate holders (testable / mockable)
            IList<MuscariableHolder> holders = _assetResolver.LoadAllAssetsAtPath<MuscariableHolder>(path)
                .OfType<MuscariableHolder>()
                .ToList();

            Debug.Log($"[FindPersistentHolderFor] Searching holders for var key='{variable?.Key}' itemID={variable?.ItemID} at assetPath='{path}'. holders.Count={holders.Count}");

            LogDiscoveredHoldersForDiagnostings();
            void LogDiscoveredHoldersForDiagnostings()
            {
                for (int i = 0; i < holders.Count; i++)
                {
                    var holderElem = holders[i];
                    int innerHash = 0;
                    string innerKey = "(null)";
                    try
                    {
                        if (holderElem.Inner != null)
                        {
                            innerHash = RuntimeHelpers.GetHashCode(holderElem.Inner);
                            innerKey = holderElem.Inner.Key;
                        }
                    }
                    catch { /* ignore */ }
                    Debug.Log($"[FindPersistentHolderFor] holder[{i}] name='{holderElem.name}' instanceId={holderElem.GetInstanceID()} itemID={holderElem.ItemID} innerKey='{innerKey}' innerHash={innerHash}");
                }
            }

            // 1) Prefer matching by stable ItemID (survives domain reloads)
            if (variable != null)
            {
                var byId = holders.FirstOrDefault(elem => elem.ItemID == variable.ItemID);
                if (byId != null)
                {
                    Debug.Log($"[FindPersistentHolderFor] Matched by ItemID: holder name='{byId.name}' instanceId={byId.GetInstanceID()} -> var key='{variable.Key}' itemID={variable.ItemID}");
                    return byId;
                }
            }

            // 2) Fallback: try matching by the Inner reference (existing behavior)
            foreach (var elem in holders)
            {
                try
                {
                    if (elem.Inner == variable)
                    {
                        Debug.Log($"[FindPersistentHolderFor] Matched by Inner reference: holder name='{elem.name}' instanceId={elem.GetInstanceID()} -> var key='{variable?.Key}'");
                        return elem;
                    }
                }
                catch
                {
                    var inner = elem.Inner;
                    if (inner == variable)
                    {
                        Debug.Log($"[FindPersistentHolderFor] (fallback) Matched by Inner reference: holder name='{elem.name}' instanceId={elem.GetInstanceID()} -> var key='{variable?.Key}'");
                        return elem;
                    }
                }
            }

            Debug.Log($"[FindPersistentHolderFor] No holder found for var key='{variable?.Key}' itemID={variable?.ItemID} at assetPath='{path}'");
            return null;
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
                Debug.Log($"[GetOrCreateRow] Reusing existing row for key='{variable.Key}' varHash={RuntimeHelpers.GetHashCode(variable)}");
                return existing;
            }

            var row = _rowFactory.Create(variable);
            if (row != null)
            {
                Debug.Log($"[GetOrCreateRow] Created new row for key='{variable.Key}' varHash={RuntimeHelpers.GetHashCode(variable)} handlerType={row.VisualHandler?.GetType().FullName}");
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
                Debug.Log($"[ReleaseRow] Releasing row for key='{variable.Key}' varHash={RuntimeHelpers.GetHashCode(variable)}");
                _activeRows.Remove(variable);
                _rowFactory?.Release(row);
            }
        }

        protected virtual void ReleaseAllActiveRows()
        {
            if (_activeRows.Count == 0) return;
            Debug.Log($"[ReleaseAllActiveRows] Releasing {_activeRows.Count} active rows");
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

            // Unsubscribe from assembly reloads
            AssemblyReloadEvents.afterAssemblyReload -= HandleAfterAssemblyReload;

            // Prevent any pending rebind retries from running
            _rebindCancelled = true;

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
                        Debug.Log($"Added test materialized container");
                    }
                    container = _testMaterializedContainer;
                }
            }

            bool tooManyChildrenOrActiveRows = container.childCount >= varsToDisplay.Count || _activeRows.Count >= varsToDisplay.Count;
            if (tooManyChildrenOrActiveRows)
            {
                //Debug.Log($"Skipping materialization: container.childCount={container.childCount}, varsToDisplay.Count={varsToDisplay.Count}, _activeRows.Count={_activeRows.Count}");
                return;
            }

            for (int i = 0; i < varsToDisplay.Count; i++)
            {
                var elem = varsToDisplay[i];
                if (elem == null) continue;

                var row = GetOrCreateRow(elem);
                if (row?.RootElement == null) continue;

                if (row.RootElement.parent == null)
                    container.Add(row.RootElement);

                var targetObj = GetBindingTarget(elem);

                // **Inject the SerializedObject into the already-initialized row**
                if (targetObj != null)
                {
                    var so = new SerializedObject(targetObj);
                    row.VisualHandler.SerializedVar = so;
                }
            }
        }

        VisualElement _testMaterializedContainer;
        #endregion

        // Called after assemblies are reloaded. The problem being addressed:
        // SerializedObjects that back each VariableRow can end up pointing at
        // re-created/different ScriptableObject instances after a domain reload.
        // Re-resolving the binding target and re-assigning the SerializedObject
        // prevents the visible UI from showing empty fields.
        protected void HandleAfterAssemblyReload()
        {
            // Reset attempts and schedule the first rebind. We use a retry loop because
            // VariableSourceAssetMaintenance (which repairs SerializeReference holders)
            // runs on the same assembly-reload event. If we rebind too early we may not
            // find persistent MuscariableHolders yet and end up with empty UI.
            _rebindAttempt = 0;
            _rebindCancelled = false;
            ScheduleRebind();
        }

        // Retry scheduling state
        private int _rebindAttempt;
        private const int _maxRebindAttempts = 6;
        private bool _rebindCancelled;

        private void ScheduleRebind()
        {
            if (_isDisposed || _rebindCancelled) return;

            // Ensure we don't spam delayCall entries: schedule a single delayed attempt.
            EditorApplication.delayCall += RebindAttempt;
        }

        private void RebindAttempt()
        {
            if (_isDisposed || _rebindCancelled) return;

            int unresolved = RebindAllActiveRowsInternal();

            // If some bindings could not be resolved, retry a few times to allow
            // VariableSourceAssetMaintenance and Unity to finish reconstructing sub-assets.
            if (unresolved > 0 && ++_rebindAttempt < _maxRebindAttempts)
            {
                // schedule another attempt on next idle frame
                EditorApplication.delayCall += RebindAttempt;
            }
            else if (unresolved > 0)
            {
                Debug.LogWarning($"VariableListView: Rebind completed with {unresolved} unresolved bindings after {_rebindAttempt} attempts. UI may appear empty for those entries.");
            }
        }

        // Returns count of unresolved variables (where no persistent holder / binding target found).
        protected int RebindAllActiveRowsInternal()
        {
            try
            {
                Debug.Log($"[RebindAllActiveRowsInternal] Starting rebind. varsToDisplay.Count={varsToDisplay?.Count ?? 0} activeRows={_activeRows.Count}");

                // If there are no variables or no factory, nothing to do.
                if (varsToDisplay == null || varsToDisplay.Count == 0 || _rowFactory == null)
                    return 0;

                // Capture snapshot of current variables (these are the authoritative objects
                // after a domain reload). We'll rebuild _activeRows to point to these.
                var currentVars = varsToDisplay.ToList();

                // Release any rows that reference old object instances / SerializedObjects.
                ReleaseAllActiveRows();

                int unresolvedCount = 0;

                // Recreate rows for the current variables and rebind their SerializedObjects.
                foreach (var variable in currentVars)
                {
                    if (variable == null)
                    {
                        Debug.Log("[RebindAllActiveRowsInternal] encountered null variable in snapshot");
                        continue;
                    }

                    Debug.Log($"[RebindAllActiveRowsInternal] Rebinding var key='{variable.Key}' varType={variable.GetType().FullName} varHash={RuntimeHelpers.GetHashCode(variable)}");

                    var row = GetOrCreateRow(variable);
                    if (row == null)
                    {
                        Debug.LogWarning($"[RebindAllActiveRowsInternal] GetOrCreateRow returned null for key='{variable.Key}'");
                        continue;
                    }

                    // Make sure the visual handler knows the current variable
                    try
                    {
                        row.VisualHandler.Variable = variable;
                    }
                    catch { /* defensive */ }

                    // Resolve the correct target object (MuscariableHolder or legacy UnityObj)
                    var targetObj = GetBindingTarget(variable);
                    if (targetObj != null)
                    {
                        string path = null;
                        try { path = _assetResolver?.GetAssetPath(targetObj); } catch { }
                        Debug.Log($"[RebindAllActiveRowsInternal] Found target for '{variable.Key}': type={targetObj.GetType().FullName} name='{targetObj.name}' instanceId={targetObj.GetInstanceID()} path='{path}'");
                        try
                        {
                            row.VisualHandler.SerializedVar = new SerializedObject(targetObj);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"VariableListView: failed to assign SerializedObject to re-created row for '{variable.Key}': {ex.Message}");
                            row.VisualHandler.SerializedVar = null;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[RebindAllActiveRowsInternal] No target found for '{variable.Key}'");
                        // If this is an asset-backed variable (not a legacy UnityObj) and we couldn't
                        // find a holder, count it unresolved so we can retry later.
                        bool expectsPersistentHolder = !(variable is UnityObj) && _variableSourceContext != null;
                        if (expectsPersistentHolder)
                            unresolvedCount++;

                        // No persistent holder; keep SerializedVar null so UI won't show stale data.
                        row.VisualHandler.SerializedVar = null;
                    }

                    // Ensure the row's visuals are refreshed so bindings are applied.
                    try
                    {
                        row.VisualHandler.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[RebindAllActiveRowsInternal] Refresh failed for '{variable.Key}': {ex.Message}");
                    }
                }

                // Finally refresh the ListView so bind/unbind flows re-run on the displayed rows.
                Refresh();

                Debug.Log($"[RebindAllActiveRowsInternal] Finished rebind. unresolvedCount={unresolvedCount}");
                return unresolvedCount;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return 0;
            }
        }

        // Public entry kept for backward compatibility: schedules the rebind flow.
        protected void RebindAllActiveRows()
        {
            _rebindAttempt = 0;
            _rebindCancelled = false;
            ScheduleRebind();
        }
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
    }

    public class VariableListViewInitArgs
    {
        public IVariableRowFactory RowFactory { get; set; }
        public ListView List { get; set; }
        public UITKLabel CountLabel { get; set; }
        public IVariableSource VariableSource { get; set; }
        public IEditorAssetResolver AssetResolver { get; set; } // optional; testing override
    }
}