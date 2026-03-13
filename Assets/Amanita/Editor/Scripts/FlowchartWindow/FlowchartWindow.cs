using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using AtMycelia.Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    public class FlowchartWindow : EditorWindow, IFlowchartHostCore
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public static FlowchartWindow S => _s;
        private static FlowchartWindow _s;

        /// <summary>
        /// Opens the FlowchartWindow, or focuses it if it's already open. Best use this instead
        /// of EditorWindow.GetWindow directly, since that would skip some important setup.
        /// </summary>
        [MenuItem("Window/Atelier Mycelia/Amanita/Flowchart Window")]
        public static void BringUp()
        {
            EnsureConfigAssetInProject();

            FlowchartWindow wnd = _s != null ?
                _s :
                GetWindow<FlowchartWindow>();
            wnd.titleContent = new GUIContent(Config.FlowchartWindowTitle);
            wnd.minSize = Config.WindowMinSize;
        }

        static void EnsureConfigAssetInProject()
        {
            Config = SOUtils.EnsureSOExists<FlowchartWindowConfig>(
                _configSubfolderPath,
                _configAssetName);
        }

        public static FlowchartWindowConfig Config { get; private set; }
        private static readonly string _configSubfolderPath = "AtMycelia/Amanita";
        private static readonly string _configAssetName = "FlowchartWindowConfig";

        public AmanitaClipboard Clipboard { get; private set; }

        public Flowchart Flowchart => _fcContext?.Flowchart;

        BlockClipboard IFlowchartHostCore.Clipboard
        {
            get => Clipboard?.BlockClipboard;
            set
            {
                CommandClipboard commandClipboard = Clipboard?.CommandClipboard ?? new CommandClipboard();
                Clipboard = new AmanitaClipboard(value, commandClipboard);
            }
        }

        bool IFlowchartHostCore.HasClipboard => Clipboard?.BlockClipboard != null &&
                                                Clipboard.BlockClipboard.HasEntries;
        protected virtual void ToggleSubs(bool on)
        {
            _eventBinder.Toggle(on);
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _sceneLifecycleCoordinator.HandleSceneLoaded(
                arg0,
                arg1,
                () => ActiveFlowchart,
                _graphicsRenderer);
        }

        private void OnSceneClosed(Scene scene)
        {
            _sceneLifecycleCoordinator.HandleSceneClosed(
                scene,
                () => ActiveFlowchart,
                _graphicsRenderer);
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

        private readonly FlowchartWindowModuleHost _moduleHost = new FlowchartWindowModuleHost();
        private readonly FlowchartWindowFlowchartStateService _flowchartStateService = new FlowchartWindowFlowchartStateService();
        private readonly FlowchartWindowPlayModeFocusService _playModeFocusService = new FlowchartWindowPlayModeFocusService();
        private readonly FlowchartWindowUiBuilder _uiBuilder = new FlowchartWindowUiBuilder();
        private readonly FlowchartWindowSelectionCoordinator _selectionCoordinator;
        private readonly FlowchartWindowEventBinder _eventBinder;
        private readonly FlowchartWindowSceneLifecycleCoordinator _sceneLifecycleCoordinator;
        private readonly FlowchartWindowPlayModeCoordinator _playModeCoordinator;
        private readonly FlowchartWindowTeardownCoordinator _teardownCoordinator;

        public FlowchartWindow()
        {
            _selectionCoordinator = new FlowchartWindowSelectionCoordinator(_flowchartStateService, _playModeFocusService);
            _sceneLifecycleCoordinator = new FlowchartWindowSceneLifecycleCoordinator(_flowchartStateService, _playModeFocusService);
            _playModeCoordinator = new FlowchartWindowPlayModeCoordinator(_playModeFocusService, _flowchartStateService, _selectionCoordinator);
            _teardownCoordinator = new FlowchartWindowTeardownCoordinator();
            _eventBinder = new FlowchartWindowEventBinder(_moduleHost, OnSelectedFlowchartChanged,
                OnSceneOpened, OnSceneClosed,
                OnSceneLoaded, OnPlayModeStateChanged,
                OnZoomChanged);
        }

        private void OnSelectedFlowchartChanged(Flowchart previous, Flowchart current)
        {
            _selectionCoordinator.HandleSelectionChanged(
                previous,
                current,
                _fcContext,
                _fcNameLabel,
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

        protected virtual void OnDisable()
        {
            Debug.Log("FlowchartWindowUitk OnDisable");
        }

        public void CreateGUI()
        {
            _moduleHost.ClearModules();
            EnsureConfigAssetInProject();

            FlowchartWindowUiBuildRequest request = new FlowchartWindowUiBuildRequest(
                rootVisualElement,
                m_VisualTreeAsset,
                ActiveFlowchart,
                MissingOverlay,
                Clipboard,
                Config,
                _blockDrawer,
                this,
                position,
                _moduleHost,
                _inputDetector);

            FlowchartWindowUiBuildResult result = _uiBuilder.Build(request);
            UxmlRoot = result.UxmlRoot;

            if (!result.HasFlowchart)
            {
                return;
            }

            Clipboard = result.Clipboard;
            _fcContext = result.FlowchartContext;
            _fcNameLabel = result.FcNameLabel;
            _zoomAmountLabel = result.ZoomLabel;

            _graphicsRenderer = result.GraphicsRenderer;
            _viewportManager = result.ViewportManager;
            _contextMenuManager = result.ContextMenuManager;
            _variablesPanel = result.VariablesPanel;

            FlowchartWindowSignals.ChangedFlowchart(null, _fcContext.Flowchart);
        }

        /// <summary>
        /// The functionally-true root, gotten from the uxml. We use this as the parent for all of our UI elements,
        /// and to determine where to show things like the missing Flowchart overlay.
        /// </summary>
        private VisualElement UxmlRoot { get; set; }

        private void OnZoomChanged(float newZoom)
        {
            _selectionCoordinator.UpdateZoom(_zoomAmountLabel, newZoom);
        }

        private Flowchart ActiveFlowchart => EditorSelectionTracker.ActiveFlowchart;
        private MissingFlowchartOverlay _missingOverlay;
        private UitkLabel _fcNameLabel, _zoomAmountLabel;

        #region Submodules
        private FcWindowGraphicsRenderer _graphicsRenderer;
        private MainViewportManager _viewportManager;
        private readonly InputSignalModule _inputDetector = new InputSignalModule();

        private ContextMenuManager _contextMenuManager;
        private FcWindowVariablesPanel _variablesPanel;
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
            Flowchart flowchart = _flowchartStateService.ResolveRefreshFlowchart(ActiveFlowchart);
            if (ActiveFlowchart != null)
            {
                Selection.activeGameObject = flowchart.gameObject;
            }

            if (flowchart)
            {
                Debug.Log("Flowchart found on refresh.");
                MissingOverlay.Hide();
                CreateGUI();
            }
            else
            {
                Debug.LogWarning("Flowchart still not found on refresh.");
            }
        }

        private FlowchartContext _fcContext;

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
            _sceneLifecycleCoordinator.HandleSceneOpened(scene, () => ActiveFlowchart,
                _fcContext, rootVisualElement,
                MissingOverlay, _graphicsRenderer);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            _playModeCoordinator.HandlePlayModeStateChanged(state, () => ActiveFlowchart,
                _fcContext, _fcNameLabel,
                _zoomAmountLabel, _graphicsRenderer);
        }

        #region Cleanup
        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(_s, this))
            {
                _s = null;
            }
            Debug.Log("FlowchartWindowUitk OnDestroy");
            ToggleSubs(false);

            FlowchartWindowTeardownRequest request = new FlowchartWindowTeardownRequest(_moduleHost, _fcContext,
                _graphicsRenderer, _viewportManager,
                _inputDetector, _contextMenuManager,
                _variablesPanel, _fcNameLabel,
                _missingOverlay);

            FlowchartWindowTeardownResult result = _teardownCoordinator.Teardown(request);

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

    