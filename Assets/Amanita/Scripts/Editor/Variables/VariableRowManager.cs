using Collections;
using MoonSharp.VsCodeDebugger.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UITKLabel = UnityEngine.UIElements.Label; // So the compiler doesn't get confused

namespace Amanita.VScripting.EditorUtils
{
    public class VariableRowManager : IDisposable
    {
        public VariableRowManager() { }

        #region Visual Handler Type Management

        static VariableRowManager()
        {
            AssemblyReloadEvents.afterAssemblyReload += RefreshHandlerLookup;
        }

        [InitializeOnLoadMethod]
        public static void RefreshHandlerLookup()
        {
            visualHandlerLookup.Clear();

            allVisualHandlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    typeof(RowVisualHandler).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    t.GetCustomAttribute<RowVisualHandlerAttribute>() != null
                );

            foreach (var handlerType in allVisualHandlerTypes)
            {
                var attr = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
                visualHandlerLookup[attr.ContentType] = handlerType;
            }
        }

        protected static IEnumerable<Type> allVisualHandlerTypes;

        // Keys are var content types, values are the types of the visual handlers meant for 
        // the corresponding keys
        protected static IDictionary<Type, Type> visualHandlerLookup = new Dictionary<Type, Type>();
        #endregion

        [InitializeOnLoadMethod]
        static void RebuildLookup()
        {

        }

        /// <summary>
        /// Also meant to be called for reuse after disposing.
        /// </summary>
        protected virtual void Initialize(VisualElement holderRoot, VisualTreeAsset template, Flowchart flowchart = null)
        {
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

        protected VisualElement _holdsManager;
        protected VisualTreeAsset _ourTemplate;
        protected Flowchart _flowchart;
        protected VisualElement _ourRoot;
        protected UITKLabel _countLabel;
        protected Button _addButton;
        protected ScrollView _listContainer;

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
        }

        protected void AddOrReuseRow(IVariable varThatNeedsRow)
        {
            VariableRow rowToUse = GetVarRow();
            // ^The var rows don't care what type we are about to ask them to represent.
            // That's for the visual-handlers to worry about, hence us not fetching rows
            // based on content type
            IRowVisualHandler handler = GetHandlerFor(varThatNeedsRow.ContentType);
            rowToUse.Init(_holdsManager, varThatNeedsRow, handler);
            _listContainer.Add(rowToUse.RootElement);
            _rowsBeingShown.Add(rowToUse);
        }

        protected virtual VariableRow GetVarRow()
        {
            VariableRow result;
            if (_rowPool.Count > 0)
            {
                result = _rowPool.Last();
                _rowPool.Remove(result);
            }
            else
            {
                result = new VariableRow();
                _allRows.Add(result);
            }

            return result;
        }

        protected readonly IList<VariableRow> _rowPool = new List<VariableRow>();
        protected readonly IList<VariableRow> _allRows = new List<VariableRow>();
        protected readonly IList<VariableRow> _rowsBeingShown = new List<VariableRow>();

        protected virtual void OnVariableRemoved(IVariable removed)
        {
            RemoveRowFor(removed);
        }

        protected virtual void RemoveRowFor(IVariable varToRemoveFor)
        {
            // If we get passed a null, then chances are tht something in the
            // Undo/Redo functionality made it so. Thus, we would remove all
            // rows that have null vars assigned
            IList<VariableRow> allToRemove = (from elem in _allRows
                                              where elem.VarToRepresent == varToRemoveFor
                                              select elem).ToList();

            foreach (VariableRow rowToRemove in allToRemove)
            {
                _holdsManager.Remove(rowToRemove.RootElement);
                rowToRemove.Dispose();
                _rowPool.Add(rowToRemove);
                _rowsBeingShown.Remove(rowToRemove);
            }
        }

        public void Refresh()
        {
            ClearAllRows();
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

            _listContainer.Clear();
            _rowPool.AddRange(_allRows);
        }

        protected readonly IList<IRowVisualHandler> _handlerPool = new List<IRowVisualHandler>();

        protected virtual IRowVisualHandler GetHandlerFor(Type contentType)
        {
            // 1. Exact lookup
            bool exactLookupSuccess = visualHandlerLookup.TryGetValue(contentType, out var handlerType);
            if (!exactLookupSuccess)
            {
                // 2. Inheritance-based lookup
                handlerType = visualHandlerLookup
                    .Where(kv => kv.Key.IsAssignableFrom(contentType))
                    .OrderByDescending(kv => kv.Key == contentType) // prefer exact, then closest
                    .Select(kv => kv.Value)
                    .FirstOrDefault();
            }

            // 3. Still no match? We don't have a generic fallback registered
            bool stillNoMatch = handlerType == null &&
                visualHandlerLookup.TryGetValue(typeof(object), out handlerType) == false;
            if (stillNoMatch)
            {
                string errorMessage = $"No RowVisualHandler found for {contentType.Name}, and no generic fallback registered.";
                throw new InvalidOperationException(errorMessage);
            }

            IRowVisualHandler result = (from elem in _handlerPool
                                        where elem.VarContentType == contentType
                                        select elem).LastOrDefault();

            bool foundOneInThePool = result != null;
            if (foundOneInThePool)
            {
                _handlerPool.Remove(result);
            }
            else
            {
                result = CreateNewHandlerFor(handlerType);
            }

            static IRowVisualHandler CreateNewHandlerFor(Type handlerType)
            {
                IRowVisualHandler result = (IRowVisualHandler)Activator.CreateInstance(handlerType);
                return result;
            }

            return result;
        }

        public virtual void Dispose()
        {
            DeregisterCallbacks();
            ClearAllRows();
            _rowPool.Clear();
            _allRows.Clear();
            _holdsManager?.Remove(_ourRoot);
            _ourRoot = null;
            _holdsManager = null;
            _countLabel = null;
            _addButton = null;
            _listContainer = null;
        }
    }

}
