using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Amanita.EditorUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amanita.VScripting.EditorUtils.FcWindow
{
    public class FlowchartWindowUitk : EditorWindow, IFlowchartHostCore
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public static FlowchartWindowUitk S => _s;
        private static FlowchartWindowUitk _s;

        [MenuItem("Window/Atelier Mycelia/Amanita/FlowchartWindowUitk")]
        public static void ShowFromMenuItem()
        {
            EnsureConfigAssetInProject();

            FlowchartWindowUitk wnd = _s != null ?
                _s :
                GetWindow<FlowchartWindowUitk>();
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
        private static readonly string _configSubfolderPath = "Amanita/Configs";
        private static readonly string _configAssetName = "FlowchartWindowUitkConfig";

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

                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
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

                AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
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

            Undo.RecordObject(Flowchart, "Deselect");
            Flowchart.ClearSelectedCommands();
            Flowchart.ClearSelectedBlocks();

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

            for (int i = 0; i < _viewportHandlers.Submodules.Count; i++)
            {
                var module = _viewportHandlers.Submodules[i];
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

        private void OnSelectedFlowchartChanged(Flowchart previous, Flowchart current)
        {
            if (_fcContext == null)
            {
                return;
            }

            Flowchart resolved;
            bool currentWasRemovedWhileWeHavePrevious = current == null && previous != null;
            if (currentWasRemovedWhileWeHavePrevious)
            {
                resolved = previous;
            }
            else
            {
                resolved = current == null ?
                    FindFirstObjectByType<Flowchart>() :
                    current;
            }

            bool changedToDiffFlowchart = !ReferenceEquals(previous, resolved); // Just in case.
            if (!changedToDiffFlowchart)
            {
                return;
            }

            if (previous != null)
            {
                previous.ClearSelectedBlocks();
                previous.ClearSelectedCommands();
            }
            _fcContext.Flowchart = resolved;
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
                _graphicsRenderer = new FcWindowGraphicsRendererUitk(_fcContext, Config.GridDrawConfig, 
                    _blockDrawer);
                _viewportHandlers = new MainViewportManager(_fcContext, Config.MinZoom, 
                    Config.MaxZoom);

                _contextMenuManager = new FlowchartContextMenuManagerUitk();
                _variablesPanel = new FcWindowVariablesPanelUitk();
            }

            RegisterModules();
            void RegisterModules()
            {
                RegisterModule(_graphicsRenderer);
                RegisterModule(_viewportHandlers);

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
                _viewportHandlers.Initialize(this);

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
        private FcWindowGraphicsRendererUitk _graphicsRenderer;
        private MainViewportManager _viewportHandlers;
        private readonly InputSignalModuleUitk _inputDetector = new InputSignalModuleUitk();

        private FlowchartContextMenuManagerUitk _contextMenuManager;
        private FcWindowVariablesPanelUitk _variablesPanel;
        #endregion
        public InputSignalModuleUitk InputSignals => _inputDetector;

        static readonly DefaultBlockDrawerUitk _blockDrawer = new DefaultBlockDrawerUitk(new BlockGraphicsGenerator());
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
            Flowchart flowchart = FindFirstObjectByType<Flowchart>();
            if (ActiveFlowchart != null)
            {
                flowchart = ActiveFlowchart;
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
            _viewportHandlers.OnGUI(Event.current);
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
            if (ActiveFlowchart != null)
            {
                ActiveFlowchart.ClearSelectedBlocks();
                ActiveFlowchart.ClearSelectedCommands();
            }
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

            Flowchart fallback = FindFirstObjectByType<Flowchart>();
            if (fallback == null)
            {
                MissingOverlay.Show(rootVisualElement);
                return;
            }

            MissingOverlay.Hide();
            Flowchart previous = _fcContext.Flowchart;
            _fcContext.Flowchart = fallback;
            _graphicsRenderer?.RefreshNow();
            if (!ReferenceEquals(previous, fallback))
            {
                FlowchartWindowSignals.ChangedFlowchart(previous, fallback);
            }
        }

        private void OnAfterAssemblyReload()
        {
            EditorApplication.delayCall += RebuildAfterAssemblyReload;
        }

        private void RebuildAfterAssemblyReload()
        {
            if (this == null)
            {
                return;
            }

            rootVisualElement.Clear();
            _blockModuleDispatcher.ClearModules();
            _moduleDispatcher.ClearModules();
            DisposeSubmodules();
            NullOutSubmodules();

            _fcContext?.Dispose();
            _fcContext = null;

            _missingOverlay?.Dispose();
            _missingOverlay = null;
            _fcNameLabel = null;

            CreateGUI();
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
            _viewportHandlers?.Dispose();

            _inputDetector.Dispose();
            _contextMenuManager?.Dispose();
            _variablesPanel?.Dispose();
        }

        void NullOutSubmodules()
        {
            _graphicsRenderer = null;
            _viewportHandlers = null;
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