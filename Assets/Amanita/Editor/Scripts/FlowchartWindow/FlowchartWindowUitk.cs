using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Amanita.EditorUtils;
using System;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartWindowUitk : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        public static FlowchartWindowUitk S => _s;
        private static FlowchartWindowUitk _s;

        [MenuItem("Window/Atelier Mycelia/Experimental/FlowchartWindowUitk")]
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

        private void OnZoomChanged(float newZoom)
        {
            _zoomAmountLabel.text = $"Zoom: {Math.Round(newZoom * 100)}%";
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
                _fcContext.FcHost = null; // TODO: assign proper host
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
                OnZoomChanged(_fcContext.Flowchart?.Zoom ?? 1f);
            }

            EnsureConfigAssetInProject(); // Since it can get nulled out during assembly reload

            CreateModules();
            void CreateModules()
            {
                _graphicsRenderer = new FcWindowGraphicsRendererUitk(_fcContext, Config.GridDrawConfig, _blockDrawer);
                _viewportHandlers = new FcWindowViewportHandlersUitk(_fcContext, Config.MinZoom, Config.MaxZoom);

                _hitDetector = new HitDetectionHandlerUitk();
                _singleClickBlockSelector = new SingleClickBlockSelector(_fcContext);
                _repaintTriggerer = new FcWindowRepaintTriggerer();
                _emptySpacePopupModule = new FlowchartContextMenuManagerUitk();
            }

            RegisterModules();
            void RegisterModules()
            {
                RegisterModule(_graphicsRenderer);
                RegisterModule(_viewportHandlers);

                RegisterModule(_singleClickBlockSelector);
                RegisterModule(_repaintTriggerer);
                RegisterModule(_inputDetector);
                RegisterModule(_emptySpacePopupModule);
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

                _singleClickBlockSelector.Initialize(this);
                _repaintTriggerer.Initialize(this);
                _inputDetector.Initialize(this);
                _hitDetector.Initialize(this);
                _emptySpacePopupModule.Initialize(this);
            }

            FlowchartWindowSignals.ChangedFlowchart(null, _fcContext.Flowchart);
        }

        /// <summary>
        /// The functionally-true root, gotten from the uxml. We use this as the parent for all of our UI elements,
        /// and to determine where to show things like the missing Flowchart overlay.
        /// </summary>
        private VisualElement UxmlRoot { get; set; }

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
        private FcWindowViewportHandlersUitk _viewportHandlers;
        private readonly InputSignalModuleUitk _inputDetector = new InputSignalModuleUitk();
        private SingleClickBlockSelector _singleClickBlockSelector;
        private FcWindowRepaintTriggerer _repaintTriggerer;
        private FlowchartContextMenuManagerUitk _emptySpacePopupModule;


        private HitDetectionHandlerUitk _hitDetector;
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

            _singleClickBlockSelector?.Dispose();
            _repaintTriggerer?.Dispose();

            _hitDetector.Dispose();
            _inputDetector.Dispose();
            _emptySpacePopupModule?.Dispose();
        }

        void NullOutSubmodules()
        {
            _hitDetector = null;
            _graphicsRenderer = null;
            _viewportHandlers = null;
            _singleClickBlockSelector = null;
            _repaintTriggerer = null;
            _emptySpacePopupModule = null;
        }

        void NullOutVisualElements()
        {
            _fcNameLabel = null;
        }
        #endregion
    }

    internal interface IModuleDispatcher
    {
        void AddModule(object module);
        void RemoveModule(object module);
        void ClearModules();
        void ToggleSubs(bool on);
    }

    internal interface IModuleDispatcher<T> : IModuleDispatcher
    {
        void AddModule(T module);
        void RemoveModule(T module);

    }
}