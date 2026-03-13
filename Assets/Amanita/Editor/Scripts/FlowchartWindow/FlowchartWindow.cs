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
            _blockModuleDispatcher.ToggleSubs(on);
            _mouseModuleDispatcher.ToggleSubs(on);

            if (on)
            {
                EditorSelectionTracker.SelectedFlowchartChanged += OnSelectedFlowchartChanged;
                FlowchartWindowSignals.ChangedFlowchart += _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.WindowPanned += _moduleDispatcher.NotifyWindowPanned;

                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorSceneManager.sceneClosed += OnSceneClosed;
                EditorSceneManager.sceneLoaded += OnSceneLoaded;

                //AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                CommandSignals.CommandSelected += _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.ZoomChanged += OnZoomChanged;

            }
            else
            {
                EditorSelectionTracker.SelectedFlowchartChanged -= OnSelectedFlowchartChanged;
                FlowchartWindowSignals.ChangedFlowchart -= _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.WindowPanned -= _moduleDispatcher.NotifyWindowPanned;

                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorSceneManager.sceneClosed -= OnSceneClosed;
                EditorSceneManager.sceneLoaded -= OnSceneLoaded;

                //AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                CommandSignals.CommandSelected -= _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.ZoomChanged -= OnZoomChanged;
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            ResetActiveFlowchartSelections();
            _graphicsRenderer?.RefreshNow();
        }

        private void OnSceneClosed(Scene scene)
        {
            ResetActiveFlowchartSelections();
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

        private readonly BlockModuleDispatcher _blockModuleDispatcher = new BlockModuleDispatcher();
        private readonly MouseModuleDispatcher _mouseModuleDispatcher = new MouseModuleDispatcher();
        private readonly FlowchartModuleDispatcher _moduleDispatcher = new FlowchartModuleDispatcher();
        private readonly FlowchartWindowFlowchartStateService _flowchartStateService = new FlowchartWindowFlowchartStateService();
        private readonly FlowchartWindowPlayModeFocusService _playModeFocusService = new FlowchartWindowPlayModeFocusService();

        private void OnSelectedFlowchartChanged(Flowchart previous, Flowchart current)
        {
            if (_fcContext == null)
            {
                return;
            }

            Flowchart resolved = _flowchartStateService.ResolveSelectionChange(previous, current);
            if (ReferenceEquals(previous, resolved))
            {
                return;
            }

            if (previous != null)
            {
                _flowchartStateService.ResetSelections(previous);
            }

            _fcContext.Flowchart = resolved;

            string cachedUid;
            if (_playModeFocusService.TryCacheFromSelection(resolved, out cachedUid))
            {
                Debug.Log($"In Play Mode - updated last-focused flowchart UID to {cachedUid}");
            }

            UpdateLabels();
            FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
        }

        private void UpdateLabels()
        {
            _fcNameLabel.text = $"FC: {FcContext.Flowchart.name}";
            _zoomAmountLabel.text = $"Zoom: {Math.Round(FcContext.Flowchart.Zoom * 100)}%";
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
            #region Clear dispatchers
            _blockModuleDispatcher.ClearModules();
            _mouseModuleDispatcher.ClearModules();
            _moduleDispatcher.ClearModules();
            #endregion

            #region Prep the root
            UxmlRoot = m_VisualTreeAsset.Instantiate();
            rootVisualElement.Add(UxmlRoot);
            UxmlRoot.pickingMode = PickingMode.Position; 
            // ^So that PointerUp events trigger properly when clicking on empty space.
            // Sub-elements can override this to receive events as normal.
            UxmlRoot.SetPadding(0);
            UxmlRoot.SetMargin(0);
            UxmlRoot.style.flexGrow = 1f;
            UxmlRoot.style.width = Length.Percent(100);
            UxmlRoot.style.height = Length.Percent(100);
            // ^To take up the full space of the window
            #endregion

            #region For when there's no Flowchart to show
            // If we have no Flowchart to look at, we cannot proceed. Show a label and return.
            if (ActiveFlowchart == null)
            {
                MissingOverlay.Show(UxmlRoot);
                return;
            }
            #endregion

            MissingOverlay.Hide();

            PrepClipboard();
            void PrepClipboard()
            {
                Clipboard ??= new AmanitaClipboard(this);
            }

            PrepFcContext();
            void PrepFcContext()
            {
                _fcContext = new FlowchartContext();
                _fcContext.Flowchart = ActiveFlowchart;//
                if (_fcContext.Flowchart == null)
                {
                    _fcContext.Flowchart = FindFirstObjectByType<Flowchart>();
                    return;
                }
                _fcContext.FcHost = this;
                _fcContext.Position = new Rect(0, 0, position.width, position.height);
                _fcContext.GridObjectSnap = 10f;
            }

            PrepFcNameLabel();
            void PrepFcNameLabel()
            {
                string labelText = "No Flowchart Selected";
                if (_fcContext.Flowchart != null)
                {
                    labelText = $"FC: {_fcContext.Flowchart.name}";
                }
                _fcNameLabel = UxmlRoot.Q<UitkLabel>("FcNameLabel");
                _fcNameLabel.text = labelText;
            }

            PrepZoomLabel();
            void PrepZoomLabel()
            {
                _zoomAmountLabel = UxmlRoot.Q<UitkLabel>("ZoomLabel");
                var newZoom = _fcContext.Flowchart != null ? 
                    _fcContext.Flowchart.Zoom : 
                    1f;
                OnZoomChanged(newZoom);
            }

            EnsureConfigAssetInProject(); // Since it can get nulled out during assembly reload

            CreateModules();
            void CreateModules()
            {
                _graphicsRenderer = new FcWindowGraphicsRenderer(_fcContext, Config.GridDrawConfig, 
                    _blockDrawer);
                _viewportManager = new MainViewportManager(_fcContext, Config.MinZoom, 
                    Config.MaxZoom);

                _contextMenuManager = new ContextMenuManager();
                _variablesPanel = new FcWindowVariablesPanel();
            }

            RegisterModules();
            void RegisterModules()
            {
                RegisterModule(_graphicsRenderer);
                RegisterModule(_viewportManager);

                RegisterModule(_contextMenuManager);
                RegisterModule(_inputDetector);
                RegisterModule(_variablesPanel);
            }

            AttachUiElements();
            void AttachUiElements()
            {
                UxmlRoot.Add(_graphicsRenderer);
                UxmlRoot.Add(_fcNameLabel);
            }

            InitSubmodules();
            void InitSubmodules()
            {
                _graphicsRenderer.Initialize(this);
                _viewportManager.Initialize(this);

                _inputDetector.Initialize(this);
                _contextMenuManager.Initialize(this);
                _variablesPanel.Initialize(this);
            }

            FlowchartWindowSignals.ChangedFlowchart(null, _fcContext.Flowchart);
        }

        /// <summary>
        /// The functionally-true root, gotten from the uxml. We use this as the parent for all of our UI elements,
        /// and to determine where to show things like the missing Flowchart overlay.
        /// </summary>
        private VisualElement UxmlRoot { get; set; }

        private void OnZoomChanged(float newZoom)
        {
            _zoomAmountLabel.text = $"Zoom: {Math.Round(newZoom * 100)}%";
        }

        private void RegisterModule(IFlowchartWindowModule module)
        {
            if (module == null)
            {
                return;
            }

            _moduleDispatcher.AddModule(module);
            _blockModuleDispatcher.AddModule(module);
            _mouseModuleDispatcher.AddModule(module);
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
            EnsureFlowchartForScene();
            ResetActiveFlowchartSelections();
            _graphicsRenderer?.RefreshNow();
        }

        private void ResetActiveFlowchartSelections()
        {
            _flowchartStateService.ResetSelections(ActiveFlowchart);
        }

        private void EnsureFlowchartForScene()
        {
            if (_fcContext == null)
            {
                return; // UI not built yet; CreateGUI will initialize.
            }

            if (_fcContext.Flowchart != null)
            {
                return; // Still valid.
            }

            Debug.Log("Seeking new flowchart for scene...");

            Flowchart lastFocusedInPlayMode;
            _playModeFocusService.TryResolveLastFocused(_flowchartStateService, out lastFocusedInPlayMode);

            bool usedPlayModeFlowchart;
            Flowchart resolved = _flowchartStateService.ResolveFlowchartForScene(_fcContext.Flowchart, lastFocusedInPlayMode, out usedPlayModeFlowchart);
            if (resolved == null)
            {
                MissingOverlay.Show(rootVisualElement);
                return;
            }

            if (usedPlayModeFlowchart)
            {
                Debug.Log("Found last-focused flowchart from play mode.");
            }

            MissingOverlay.Hide();
            Flowchart previous = _fcContext.Flowchart;
            _fcContext.Flowchart = resolved;
            _graphicsRenderer?.RefreshNow();
            if (!ReferenceEquals(previous, resolved))
            {
                FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode &&
                state != PlayModeStateChange.EnteredPlayMode &&
                state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                string cachedUid;
                if (_playModeFocusService.TryCacheFromActiveFlowchart(ActiveFlowchart, out cachedUid))
                {
                    Debug.Log($"Entered play mode - cached last-focused flowchart UID as {cachedUid}");
                }
            }
                        
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += () =>
                {
                    if (_fcContext == null)
                    {
                        return;
                    }

                    Flowchart lastFocusedInPlayMode;
                    if (_playModeFocusService.TryResolveLastFocused(_flowchartStateService, out lastFocusedInPlayMode))
                    {
                        Debug.Log($"Found last-focused flowchart from play mode on exit: {lastFocusedInPlayMode.name}");
                        Selection.activeGameObject = lastFocusedInPlayMode.gameObject;
                        FcContext.Flowchart = lastFocusedInPlayMode;
                        UpdateLabels();
                    }
                    else if (_playModeFocusService.HasCachedFocus)
                    {
                        Debug.LogWarning("Could not find last-focused flowchart from play mode on exit.");
                    }

                    _graphicsRenderer?.ResetVisuals();
                };
            }
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

            _blockModuleDispatcher.ClearModules();
            _mouseModuleDispatcher.ClearModules();
            _moduleDispatcher.ClearModules();
            _fcContext?.Dispose();
            _fcContext = null;

            DisposeSubmodules();
            NullOutSubmodules();

            _fcNameLabel?.RemoveFromHierarchy();
            _missingOverlay?.Dispose();
            _missingOverlay = null;
            NullOutVisualElements();
        }

        void DisposeSubmodules()
        {
            _graphicsRenderer?.Dispose();
            _viewportManager?.Dispose();

            _inputDetector.Dispose();
            _contextMenuManager?.Dispose();
            _variablesPanel?.Dispose();
        }

        void NullOutSubmodules()
        {
            _graphicsRenderer = null;
            _viewportManager = null;
            _contextMenuManager = null;
            _variablesPanel = null;
        }

        void NullOutVisualElements()
        {
            _fcNameLabel = null;
        }
        #endregion
    }

    
}