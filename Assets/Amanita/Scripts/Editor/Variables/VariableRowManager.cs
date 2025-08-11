using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label; // So the compiler doesn't get confused
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRowManager : IDisposable
    {
        public VariableRowManager(IRowVisualHandlerResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
        }

        protected IRowVisualHandlerResolver _handlerResolver;
        #region Visual Handler Type Management

        [InitializeOnLoadMethod]
        protected static void InitializeHandlerLookup()
        {
            // Always rebuild lookup right now
            RefreshHandlerLookup();

            // Ensure we only subscribe once
            AssemblyReloadEvents.afterAssemblyReload -= RefreshHandlerLookup;
            AssemblyReloadEvents.afterAssemblyReload += RefreshHandlerLookup;
        }

        protected static readonly object _handlerLookupLock = new object();

        public static void RefreshHandlerLookup()
        {
            var visHandlerType = typeof(RowVisualHandler);

            // 1) Snapshot types safely (avoid ReflectionTypeLoadException)
            var discovered = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(typeEl =>
                    visHandlerType.IsAssignableFrom(typeEl) &&
                    !typeEl.IsAbstract &&
                    typeEl.GetCustomAttribute<RowVisualHandlerAttribute>() != null)
                .ToArray(); // snapshot

            // 2) Precompute the pairs outside the lock
            var pairs = new List<KeyValuePair<Type, Type>>(discovered.Length);
            foreach (var handlerType in discovered)
            {
                var attr = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
                pairs.Add(new KeyValuePair<Type, Type>(attr.ContentType, handlerType));
            }

            // 3) Atomic update under the lock (keep same dictionary instance)
            lock (_handlerLookupLock)
            {
                visualHandlerLookup.Clear();
                foreach (var kv in pairs)
                    visualHandlerLookup[kv.Key] = kv.Value;

                allVisualHandlerTypes = discovered; // consistent with the lookup now
            }
        }

        protected static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(typeEl => typeEl != null); }
        }

        protected static IEnumerable<Type> allVisualHandlerTypes;

        // Keys are var content types (like for floats, ints, etc), values are the types
        // of the visual handlers meant for the corresponding keys
        protected static IDictionary<Type, Type> visualHandlerLookup = new Dictionary<Type, Type>(new TypeNameComparer());

#endregion

        public virtual void Init(VRowManagerInitArgs initArgs)
        {
            _isDisposed = false;
            DeregisterCallbacks();
            InitVisuals(initArgs);
            _handlerPool = new RowVisualHandlerPool(_handlerResolver, visualHandlerLookup);
            ListenForEvents();
            Refresh();
        }

        protected bool _isDisposed;

        protected virtual void InitVisuals(VRowManagerInitArgs initArgs)
        {
            _holdsManager = initArgs.HoldsManager;
            _ourRoot = initArgs.Root;
            _listContainer = initArgs.ListContainer;
            _countLabel = initArgs.CountLabel;
            _addButton = initArgs.AddButton;
            _flowchart = initArgs.Flowchart;
        }

        protected VisualElement _holdsManager; 
        // ^We expect this to be FlowchartWindow or something meant to fulfill its purpose
        protected VisualTreeAsset _ourTemplate;
        protected Flowchart _flowchart;
        protected VisualElement _ourRoot;
        protected UITKLabel _countLabel;
        protected Button _addButton;
        protected VisualElement _listContainer;
        protected RowVisualHandlerPool _handlerPool;

        public virtual void RegisterAndAddToRoot(VisualElement toHoldManager)
        {
            if ( (_holdsManager != null && _holdsManager != toHoldManager) &&
                _ourRoot != null && _ourRoot.parent != null)
            {
                _holdsManager.Remove(_ourRoot);
            }

            _holdsManager = toHoldManager;
            _holdsManager.Add(_ourRoot);
        }

        protected virtual void DeregisterCallbacks()
        {
            if (_flowchart == null)
            {
                return;
            }
            
            _addButton.clicked -= OnAddClicked;
            _flowchart.VariableAdded -= OnVariableAdded;
            _flowchart.VariableRemoved -= OnVariableRemoved;
        }

        protected virtual void ListenForEvents()
        {
            if (_flowchart == null)
            {
                return;
            }

            _addButton.clicked += OnAddClicked;
            _flowchart.VariableAdded += OnVariableAdded;
            _flowchart.VariableRemoved += OnVariableRemoved;
        }

        protected void OnAddClicked()
        {
            /* show add dialog, then Refresh */
        }

        protected virtual void OnVariableAdded(IVariable added)
        {
            AddOrReuseRow(added);
            RefreshCountLabel();
        }

        // Involves lifetime registry mutation
        protected void AddOrReuseRow(IVariable varThatNeedsRow)
        {
            VariableRow rowToUse = _rowPool.GetOrCreate();
            // ^The var rows don't care what type we are about to ask them to represent.
            // That's for the visual-handlers to worry about, hence us not fetching rows
            // based on content type

            IRowVisualHandler handler = _handlerPool.GetHandlerFor(varThatNeedsRow.ContentType,
                _holdsManager, varThatNeedsRow);
            rowToUse.Init(_holdsManager, varThatNeedsRow, handler);

#if DEV_DIAGNOSTICS
            if (rowToUse.RootElement == null)
                Debug.LogWarning($"Row created with null RootElement for var '{varThatNeedsRow?.Key}'");
#endif

            _listContainer.Add(rowToUse.RootElement);
            _allRows.Add(rowToUse);
        }

        protected readonly VariableRowPool _rowPool = new VariableRowPool();
        protected readonly HashSet<VariableRow> _allRows = new HashSet<VariableRow>();
        // ^A sort of lifetime registry of rows, be they pooled or visible. Should only be
        // cleared by the Dispose method

        protected virtual void OnVariableRemoved(IVariable removed)
        {
            RemoveRowFor(removed);
            RefreshCountLabel();
        }

        protected virtual void RemoveRowFor(IVariable varToRemoveFor)
        {
            // If we get passed a null, then chances are that something in the
            // Undo/Redo functionality made it so. Thus, we would remove all
            // rows that have null vars assigned
            VariableRow rowToRemove = (from elem in _allRows
                                       where elem.VarToRepresent == varToRemoveFor
                                       select elem).FirstOrDefault();

            if (rowToRemove == null)
            {
                return;
            }

#if DEV_DIAGNOSTICS
            if (rowToRemove.RootElement == null)
                Debug.LogError($"RemoveRowFor: Row RootElement is null for var '{varToRemoveFor?.Key}'");
#endif
            ReleaseRow(rowToRemove);
        }

        public void Refresh()
        {
            if (_flowchart == null)
            {
                return;
            }

            ReleaseAllRows();
            foreach (var varToShow in _flowchart.Variables)
                AddOrReuseRow(varToShow);
            RefreshCountLabel();
        }

        protected virtual void RefreshCountLabel()
        {
            _countLabel.text = $"Count: {_listContainer.childCount}";
        }

        protected virtual void ReleaseAllRows()
        {
            var snapshot = VisibleRows;
            foreach (var elem in EnumerateVisibleRows())
            {
                ReleaseRow(elem);
            }
        }

        // Enumerates without allocation
        public IEnumerable<VariableRow> EnumerateVisibleRows()
        {
            var container = _listContainer; // Avoids capturing the field in the iterator state
            foreach (var row in _allRows)
            {
                if (row.RootElement?.parent == container)
                    yield return row;
            }
        }


        protected virtual void ReleaseRow(VariableRow toRelease)
        {
            if (toRelease == null || toRelease.RootElement?.parent != _listContainer)
            {
                return;
            }

            var root = toRelease.RootElement;
            if (root?.parent != null)
            {
                root.RemoveFromHierarchy();
            }

            var handler = toRelease?.VisualHandler;
            if (handler != null)
                _handlerPool.Release(handler);

            _rowPool.Release(toRelease);
        }

        // Involves lifetime registry mutation
        public virtual void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            DeregisterCallbacks();
            ReleaseAllRows();
            RefreshCountLabel();
            _rowPool.Clear();
            _handlerPool.Clear();
            _allRows.Clear();
            if (_ourRoot != null)
            {
                if (_holdsManager != null && _holdsManager.Contains(_ourRoot))
                {
                    _holdsManager.Remove(_ourRoot);
                }
            }

            _holdsManager = null;
            _ourRoot = null;
            _countLabel = null;
            _addButton = null;
            _listContainer = null;
            _isDisposed = true;
        }

        public virtual VariableRow GetVisibleRowAt(int index)
        {
            var rows = VisibleRows; // Best work with just one copy per call of this func for the sake of performance
            if (rows.Count > index)
            {
                return rows[index];
            }
            else
            {
                return null;
            }
        }

        public IList<VariableRow> VisibleRows =>
            EnumerateVisibleRows().ToList();

        public virtual int VisibleRowCount
        {
            get
            {
                int count = 0;
                var container = _listContainer;
                foreach (var row in _allRows)
                    if (row.RootElement?.parent == container)
                        count++;
                return count;

            }
        }
        // ^For performance, we're not converting things to a list before getting the count

        #region For Testing Only
        // Can't use InternalsVisibleTo on properties, so...
        public int PooledRowCount => _rowPool.Count;
        public int PooledHandlerCount => _handlerPool.PooledHandlerCount;
        public RowVisualHandlerPool HandlerPool => _handlerPool;

#endregion
    }

}
