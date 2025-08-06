using Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label; // So the compiler doesn't get confused
using UnityEngine;
using System.Runtime.CompilerServices;

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

        public static void RefreshHandlerLookup()
        {
            visualHandlerLookup.Clear();

            Type rvHandlerGeneralType = typeof(RowVisualHandler);
            allVisualHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(typeToCheck =>
                    rvHandlerGeneralType.IsAssignableFrom(typeToCheck) &&
                    !typeToCheck.IsAbstract &&
                    typeToCheck.GetCustomAttribute<RowVisualHandlerAttribute>() != null
                );

            foreach (var handlerType in allVisualHandlerTypes)
            {
                var attr = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
                Type contentType = attr.ContentType;
                visualHandlerLookup[contentType] = handlerType;
            }

            Debug.Log("RefreshHandlerLookup done");
        }

        protected static IEnumerable<Type> allVisualHandlerTypes;

        // Keys are var content types (like for floats, ints, etc), values are the types
        // of the visual handlers meant for the corresponding keys
        protected static IDictionary<Type, Type> visualHandlerLookup = new Dictionary<Type, Type>(new TypeNameComparer());

        class TypeNameComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
              => String.Equals(x?.AssemblyQualifiedName, y?.AssemblyQualifiedName, StringComparison.Ordinal);

            public int GetHashCode(Type t)
              => t.AssemblyQualifiedName.GetHashCode();
        }

        #endregion

        /// <summary>
        /// Also meant to be called for reuse after disposing.
        /// </summary>
        public virtual void Init(VisualElement holderRoot, VisualTreeAsset template, Flowchart flowchart = null)
        {
            _isDisposed = false;
            DeregisterCallbacks(); // In case we are switching Flowcharts

            this._holdsManager = holderRoot; // At this time, we expect this to be the root for the FlowchartWindow
            _ourTemplate = template;
            _flowchart = flowchart;

            _ourRoot = _ourTemplate.CloneTree();
            _countLabel = _ourRoot.Q<UITKLabel>("varCountLabel");
            _addButton = _ourRoot.Q<Button>("addVarButton");
            _listContainer = _ourRoot.Q<ScrollView>("rowList");
            _holdsManager.Add(_ourRoot);

            ListenForEvents();
            Refresh();
        }

        protected bool _isDisposed;

        public virtual void Init(VariableRowInitArgs initArgs)
        {
            _isDisposed = false;
            DeregisterCallbacks();
            _holdsManager = initArgs.Root;
            _listContainer = initArgs.ListContainer;
            _countLabel = initArgs.CountLabel;
            _addButton = initArgs.AddButton;
            _flowchart = initArgs.Flowchart;
            _handlerPool = new RowVisualHandlerPool(_handlerResolver, visualHandlerLookup);
            ListenForEvents();
            Refresh();
        }

        protected VisualElement _holdsManager;
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
            _countLabel.text = _flowchart.VariableCount.ToString();
        }

        protected void AddOrReuseRow(IVariable varThatNeedsRow)
        {
            VariableRow rowToUse = _rowPool.GetOrCreate();
            // ^The var rows don't care what type we are about to ask them to represent.
            // That's for the visual-handlers to worry about, hence us not fetching rows
            // based on content type

            IRowVisualHandler handler = _handlerPool.GetHandlerFor(varThatNeedsRow.ContentType,
                _holdsManager, varThatNeedsRow);
            rowToUse.Init(_holdsManager, varThatNeedsRow, handler);
            _listContainer.Add(rowToUse.RootElement);
            _rowsBeingShown.Add(rowToUse);
            _allRows.Add(rowToUse);
        }

        protected readonly VariableRowPool _rowPool = new VariableRowPool();
        protected readonly HashSet<VariableRow> _allRows = new HashSet<VariableRow>();
        protected readonly IList<VariableRow> _rowsBeingShown = new List<VariableRow>();

        protected virtual void OnVariableRemoved(IVariable removed)
        {
            RemoveRowFor(removed);
            _countLabel.text = _flowchart.VariableCount.ToString();
        }

        protected virtual void RemoveRowFor(IVariable varToRemoveFor)
        {
            // If we get passed a null, then chances are that something in the
            // Undo/Redo functionality made it so. Thus, we would remove all
            // rows that have null vars assigned
            VariableRow rowToRemove = (from elem in _allRows
                                       where elem.VarToRepresent == varToRemoveFor
                                       select elem).First();

            // Visuals
            _ourRoot?.Remove(rowToRemove.RootElement);
            _listContainer.Remove(rowToRemove.RootElement);
            _rowsBeingShown.Remove(rowToRemove);

            // Pooling
            var handler = rowToRemove.VisualHandler;
            _handlerPool.Release(handler);
            _rowPool.Release(rowToRemove);
        }

        public void Refresh()
        {
            HideAndReturnAllRowsToPool();
            IList<IVariable> fcVars = _flowchart.Variables;
            _countLabel.text = fcVars.Count.ToString();
            foreach (var varToShow in fcVars)
                AddOrReuseRow(varToShow);
        }

        /// <summary>
        /// Gets all rows to stop representing vars. Also empties the list container, making
        /// the rows return to the pool.
        /// </summary>
        protected void ClearAllRows()
        {
            foreach (VariableRow row in _allRows)
            {
                row.Clear();
            }
        }

        protected virtual void HideAndReturnAllRowsToPool()
        {
            _rowsBeingShown.Clear();
            _listContainer.Clear();
            _rowPool.ReleaseRange(_allRows);

        }

        public virtual void Dispose()
        {
            if (_isDisposed )
            {
                return;
            }

            DeregisterCallbacks();
            HideAndReturnAllRowsToPool();
            _countLabel.text = "0";
            _rowPool.Clear();
            _handlerPool.Clear();
            _allRows.Clear();
            if (_ourRoot != null)
            {
                _holdsManager?.Remove(_ourRoot);
                _holdsManager = null;
                _ourRoot = null;
            }

            _countLabel = null;
            _addButton = null;
            _listContainer = null;
            _isDisposed = true;
        }

        public virtual VariableRow GetVisibleRowAt(int index)
        {
            if (_rowsBeingShown.Count > index)
            {
                return _rowsBeingShown[index];
            }
            else
            {
                return null;
            }
        }

        #region For Testing Only
#if UNITY_EDITOR
        public int PooledRowCount => _rowPool.Count;
        public int PooledHandlerCount => _handlerPool.PooledHandlerCount;
        public RowVisualHandlerPool HandlerPool => _handlerPool;
#endif

#endregion
    }

    /// <summary>
    /// Holds the UI elements to get a VariableRowManager to do its thing with.
    /// </summary>
    public class VariableRowInitArgs
    {
        public VisualElement Root { get; set; }
        public VisualElement ListContainer { get; set; }
        public UITKLabel CountLabel { get; set; }
        public Flowchart Flowchart { get; set; }
        public Button AddButton { get; set; }
    }
}
