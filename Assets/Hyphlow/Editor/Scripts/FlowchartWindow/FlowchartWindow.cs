using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public class FlowchartWindow : EditorWindow, IFlowchartHostCore
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        /// <summary>
        /// Opens the FlowchartWindow, or focuses it if it's already open. Best use this instead
        /// of EditorWindow.GetWindow directly, since that would skip some important setup.
        /// </summary>
        [MenuItem("Window/Atelier Mycelia/Amanita/Flowchart Window")]
        public static void BringUp()
        {
            FlowchartWindow wnd = _s != null ?
                _s :
                GetWindow<FlowchartWindow>();
            wnd.titleContent = new GUIContent(Config.FlowchartWindowTitle);
            wnd.minSize = Config.WindowMinSize;
            wnd.Focus();
        }

        public static FlowchartWindow S => _s;
        private static FlowchartWindow _s;

        public static FlowchartWindowConfig Config => HyphlowEditorSysAssets.FcwConfig;

        public Flowchart Flowchart => _fcContext?.Flowchart;
        private FlowchartContext _fcContext;

        protected virtual void ToggleSubs(bool on)
        {
            _eventBinder.Toggle(on);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetActiveFlowchartSelections(() => ActiveFlowchart);
            _graphicsRenderer?.RefreshNow();
        }

        private void OnSceneClosed(Scene scene)
        {
            ResetActiveFlowchartSelections(() => ActiveFlowchart);
            _graphicsRenderer?.RefreshNow();
        }

        public Block CreateBlock(Flowchart fc, Vector2 pos)
        {
            if (fc == null)
            {
                return null;
            }

            Block newBlock = fc.CreateBlock(pos);
            UpdateBlockCollection();
            Undo.RegisterCreatedObjectUndo(newBlock, "New Block");

            fc.AddToSelection(newBlock);
            return newBlock;
        }

        public void DeselectAll()
        {
            if (Flowchart == null)
            {
                return;
            }

            Undo.RecordObject(Flowchart, $"Deselect in {Flowchart.name}");
            Flowchart.DeselectAll();

            if (Selection.activeGameObject != Flowchart.gameObject)
            {
                Selection.activeGameObject = Flowchart.gameObject;
            }
        }

        public void UpdateBlockCollection()
        {
            _graphicsRenderer?.RefreshNow();
        }

        public T GetComponent<T>() where T : IFcWindowComponent
        {
            T result = default;
            for (int i = 0; i < _graphicsRenderer.Submodules.Count; i++)
            {
                var module = _graphicsRenderer.Submodules[i];
                if (module is T moduleAsT)
                {
                    result = moduleAsT;
                    break;
                }
            }

            for (int i = 0; i < _viewportManager.Submodules.Count; i++)
            {
                var module = _viewportManager.Submodules[i];
                if (module is T moduleAsT)
                {
                    result = moduleAsT;
                    break;
                }
            }
            return result;
        }

        public Vector2 GetBlockCenter(IReadOnlyCollection<Block> blocks)
        {
            if (blocks == null || blocks.Count == 0)
            {
                return Vector2.zero;
            }

            var firstBlock = blocks.First();
            Vector2 min = firstBlock._NodeRect.min;
            Vector2 max = firstBlock._NodeRect.max;

            foreach (var blockEl in blocks)
            {
                min.x = Mathf.Min(min.x, blockEl._NodeRect.min.x);
                min.y = Mathf.Min(min.y, blockEl._NodeRect.min.y);
                max.x = Mathf.Max(max.x, blockEl._NodeRect.max.x);
                max.y = Mathf.Max(max.y, blockEl._NodeRect.max.y);
            }

            return (min + max) * 0.5f;
        }

        private readonly FcwModuleHost _moduleHost = new FcwModuleHost();
        
        public FlowchartWindow()
        {
            _selectionCoordinator = new FcwSelectionCoordinator();
            _playModeCoordinator = new FcwPlayModeCoordinator();
            _teardownCoordinator = new FcwTeardownCoordinator();
            _clipboardCoordinator = new FcwClipboardCoordinator();
            _refreshCoordinator = new FcwRefreshCoordinator();
            _eventBinder = new FcwEventBinder(_moduleHost, OnSelectedFlowchartChanged,
                OnSceneOpened, OnSceneClosed,
                OnSceneLoaded, OnPlayModeStateChanged,
                OnZoomChanged);
        }

        private readonly FcwSelectionCoordinator _selectionCoordinator;
        private readonly FcwPlayModeCoordinator _playModeCoordinator;
        private readonly FcwTeardownCoordinator _teardownCoordinator;
        private readonly FcwClipboardCoordinator _clipboardCoordinator;
        private readonly FcwRefreshCoordinator _refreshCoordinator;
        private readonly FcwEventBinder _eventBinder;

        private void OnSelectedFlowchartChanged(Flowchart previous, Flowchart current)
        {
            _selectionCoordinator.HandleSelectionChanged(previous, current,
                ref _fcContext, _fcNameLabel,
                _zoomAmountLabel);
        }

        public FlowchartContext FcContext => _fcContext;

        protected virtual void OnEnable()
        {
            if (_s != null && _s != this)
            {
                Close();
                return;
            }

            _s = this;

            ToggleSubs(true);
        }

        public void CreateGUI()
        {
            _moduleHost.ClearModules();
            Clipboard = _clipboardCoordinator.EnsureClipboard(Clipboard, this);

            FcwUiBuildRequest request = new FcwUiBuildRequest(rootVisualElement, m_VisualTreeAsset, ActiveFlowchart,
                MissingOverlay, Clipboard, Config,
                _blockDrawer, this, position,
                _moduleHost, _inputDetector);

            FcwUiBuildResult result = _uiBuilder.Build(request);
            UxmlRoot = result.UxmlRoot; 
            // ^Need to fetch this before the rest of the results, since it's needed for things
            // like the Missing Flowchart Overlay.

            if (!result.HasFlowchart)
            {
                return;
            }

            FetchFromResults();
            void FetchFromResults()
            {
                Clipboard = result.Clipboard;
                _fcContext = result.FlowchartContext;
                _fcNameLabel = result.FcNameLabel;
                _zoomAmountLabel = result.ZoomLabel;

                _graphicsRenderer = result.GraphicsRenderer;
                _viewportManager = result.ViewportManager;
                _contextMenuManager = result.ContextMenuManager;
                _variablesPanel = result.VariablesPanel;
            }

            FlowchartWindowSignals.ChangedFlowchart(null, _fcContext.Flowchart);
        }

        bool IFlowchartHostCore.HasClipboard => _clipboardCoordinator.HasClipboard(Clipboard);
        BlockClipboard IFlowchartHostCore.Clipboard
        {
            get => _clipboardCoordinator.GetBlockClipboard(Clipboard);
            set => Clipboard = _clipboardCoordinator.SetBlockClipboard(Clipboard, value);
        }

        public AmanitaClipboard Clipboard { get; private set; }

        private readonly FcwUiBuilder _uiBuilder = new FcwUiBuilder();

        /// <summary>
        /// The functionally-true root, gotten from the uxml. We use this as the parent for all of our UI elements,
        /// and to determine where to show things like the missing Flowchart overlay.
        /// </summary>
        private VisualElement UxmlRoot { get; set; }

        private void OnZoomChanged(float newZoom)
        {
            _selectionCoordinator.UpdateZoom(_zoomAmountLabel, newZoom);
        }

        private Flowchart ActiveFlowchart
        {
            get
            {
                Flowchart result = EditorSelectionTracker.ActiveFlowchart;

                if (result == null)
                {
                    result = EditorSelectionTracker.LastActiveFlowchart;
                }

                return result;
            }
        }
        private MissingFlowchartOverlay _missingOverlay;
        private UitkLabel _fcNameLabel, _zoomAmountLabel;

        #region Submodules
        private FcwGraphicsRenderer _graphicsRenderer;
        private MainViewportManager _viewportManager;
        private readonly InputSignalModule _inputDetector = new InputSignalModule();

        private ContextMenuManager _contextMenuManager;
        private FcwVariablesPanel _variablesPanel;
        #endregion
        public InputSignalModule InputSignals => _inputDetector;

        static readonly DefaultBlockDrawer _blockDrawer = new DefaultBlockDrawer(new BlockGraphicsGenerator());
        private MissingFlowchartOverlay MissingOverlay
        {
            get
            {
                _missingOverlay ??= new MissingFlowchartOverlay(OnRefreshButtonClicked);
                return _missingOverlay;
            }
        }

        public VisualElement RootVisualElement
        {
            get
            {
                if (_s == null)
                {
                    return null;
                }

                return UxmlRoot;
            }
        }

        void OnRefreshButtonClicked()
        {
            _refreshCoordinator.HandleRefresh(() => ActiveFlowchart, MissingOverlay, CreateGUI);
        }

        private void OnGUI()
        {
            bool inValidState = _fcContext != null && _fcContext.Flowchart != null;
            if (!inValidState)
            {
                return;
            }
            _inputDetector.OnGUI(Event.current);
            _viewportManager.OnGUI(Event.current);
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            // Both this and OnSceneClosed can also execute in response to the user
            // right-clicking the scene in the hierarchy and selecting "Discard changes".
            // In that case, the active Flowchart may be destroyed without us knowing,
            // so we need to check validity and update accordingly.
            EnsureFlowchartForScene(_fcContext, rootVisualElement, MissingOverlay, _graphicsRenderer);
            ResetActiveFlowchartSelections(() => ActiveFlowchart);
            _graphicsRenderer?.RefreshNow();
        }

        private void ResetActiveFlowchartSelections(Func<Flowchart> activeFlowchartGetter)
        {
            if (activeFlowchartGetter == null)
            {
                return;
            }

            Flowchart flowchart = activeFlowchartGetter();
            if (flowchart == null)
            {
                return;
            }

            flowchart.ClearSelectedBlocks();
            flowchart.ClearSelectedCommands();
        }

        private void EnsureFlowchartForScene(FlowchartContext context, VisualElement rootVisualElement,
            MissingFlowchartOverlay missingOverlay, FcwGraphicsRenderer graphicsRenderer)
        {
            if (context == null)
            {
                return; // UI not built yet; CreateGUI will initialize.
            }

            if (context.Flowchart != null)
            {
                return; // Still valid.
            }

            Debug.Log("Seeking new flowchart for scene...");

            Flowchart resolved = EditorSelectionTracker.ResolveActiveFlowchart();
            if (resolved == null)
            {
                missingOverlay?.Show(rootVisualElement);
                return;
            }

            missingOverlay?.Hide();
            Flowchart previous = context.Flowchart;
            context.Flowchart = resolved;
            graphicsRenderer?.RefreshNow();
            if (!ReferenceEquals(previous, resolved))
            {
                FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            _playModeCoordinator.HandlePlayModeStateChanged(state, () => ActiveFlowchart, _fcContext,
                _fcNameLabel, _zoomAmountLabel, _graphicsRenderer);
        }

        #region Cleanup
        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(_s, this))
            {
                _s = null;
            }
            ToggleSubs(false);

            FcwTeardownRequest request = new FcwTeardownRequest(_moduleHost, _fcContext, _graphicsRenderer, 
                _viewportManager, _inputDetector, _contextMenuManager,
                _variablesPanel, _fcNameLabel, _missingOverlay);

            FcwTeardownResult result = _teardownCoordinator.Teardown(request);

            _fcContext = result.FlowchartContext;
            _graphicsRenderer = result.GraphicsRenderer;
            _viewportManager = result.ViewportManager;
            _contextMenuManager = result.ContextMenuManager;
            _variablesPanel = result.VariablesPanel;
            _fcNameLabel = result.FcNameLabel;
            _missingOverlay = null;
        }
        #endregion
    }
}