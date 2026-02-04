using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartWindowUitk : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private static FlowchartWindowUitk _s;

        public static FlowchartWindowUitk S => _s;

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
            if (on)
            {
                EditorSelectionTracker.SelectedFlowchartChanged += OnSelectedFlowchartChanged;

                BlockSignals.BlockCreated += _moduleDispatcher.NotifyBlockCreated;
                BlockSignals.BlockClicked += _moduleDispatcher.NotifyBlockClicked;
                BlockSignals.BlockSelected += _moduleDispatcher.NotifyBlockSelected;
                BlockSignals.PreBlockDelete += _moduleDispatcher.NotifyPreBlockDeleted;
                BlockSignals.PreMultiBlockDelete += _moduleDispatcher.NotifyPreMultiBlockDeleted;

                FlowchartWindowSignals.LeftClicked += _moduleDispatcher.NotifyLeftClick;
                FlowchartWindowSignals.RightClicked += _moduleDispatcher.NotifyRightClick;
                FlowchartWindowSignals.DoubleClicked += _moduleDispatcher.NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved += _moduleDispatcher.NotifyScrollWheelMoved;
                FlowchartWindowSignals.ScrollWheelDragged += _moduleDispatcher.NotifyScrollWheelDragged;
                FlowchartWindowSignals.EmptySpaceClicked += _moduleDispatcher.NotifyEmptySpaceClicked;

                FlowchartWindowSignals.ChangedFlowchart += _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.BlocksCopied += _moduleDispatcher.NotifyBlocksCopied;
                FlowchartWindowSignals.CommandSelected += _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.WindowPanned += _moduleDispatcher.NotifyWindowPanned;

                EditorSceneManager.sceneOpened += OnSceneOpened;

                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            }
            else
            {
                EditorSelectionTracker.SelectedFlowchartChanged -= OnSelectedFlowchartChanged;

                BlockSignals.BlockCreated -= _moduleDispatcher.NotifyBlockCreated;
                BlockSignals.BlockClicked -= _moduleDispatcher.NotifyBlockClicked;
                BlockSignals.BlockSelected -= _moduleDispatcher.NotifyBlockSelected;
                BlockSignals.PreBlockDelete -= _moduleDispatcher.NotifyPreBlockDeleted;
                BlockSignals.PreMultiBlockDelete -= _moduleDispatcher.NotifyPreMultiBlockDeleted;

                FlowchartWindowSignals.LeftClicked -= _moduleDispatcher.NotifyLeftClick;
                FlowchartWindowSignals.RightClicked -= _moduleDispatcher.NotifyRightClick;
                FlowchartWindowSignals.DoubleClicked -= _moduleDispatcher.NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved -= _moduleDispatcher.NotifyScrollWheelMoved;
                FlowchartWindowSignals.ScrollWheelDragged -= _moduleDispatcher.NotifyScrollWheelDragged;
                FlowchartWindowSignals.EmptySpaceClicked -= _moduleDispatcher.NotifyEmptySpaceClicked;

                FlowchartWindowSignals.ChangedFlowchart -= _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.BlocksCopied -= _moduleDispatcher.NotifyBlocksCopied;
                FlowchartWindowSignals.CommandSelected -= _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.WindowPanned -= _moduleDispatcher.NotifyWindowPanned;

                EditorSceneManager.sceneOpened -= OnSceneOpened;

                AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

            }
        }

        private readonly FlowchartModuleDispatcher _moduleDispatcher = new FlowchartModuleDispatcher();

        private void OnSelectedFlowchartChanged(Flowchart previous, Flowchart current)
        {
            if (_fcContext == null)
            {
                return;
            }

            Flowchart resolved = current == null ?
                FindFirstObjectByType<Flowchart>() :
                current;

            bool changedToDiffFlowchart = !ReferenceEquals(previous, resolved); // Just in case.
            if (!changedToDiffFlowchart)
            {
                return;
            }

            _fcContext.Flowchart = resolved;
            FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
        }

        protected virtual void OnDisable()
        {
            Debug.Log("FlowchartWindowUitk OnDisable");
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(_s, this))
            {
                _s = null;
            }

            ToggleSubs(false);

            _moduleDispatcher.ClearModules();//
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
            _scrollPosResetter?.Dispose();
            _blockRenderer?.Dispose();
            _zoomHandler?.Dispose();
        }

        void NullOutSubmodules()
        {
            _scrollPosResetter = null;
            _gridRenderer = null;
            _panHandler = null;
            _blockRenderer = null;
            _blockClickSelectionSyncer = null;
            _repaintTriggerer = null;
            _zoomHandler = null;
        }

        void NullOutVisualElements()
        {
            _fcNameLabel = null;
        }

        public void CreateGUI()
        {
            _moduleDispatcher.ClearModules();
            VisualElement root = rootVisualElement;

            // If we have no Flowchart to look at, we cannot proceed. Show a label and return.
            if (ActiveFlowchart == null)
            {
                MissingOverlay.Show(root);
                return;
            }

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
                _fcNameLabel = new UitkLabel(labelText);
                _fcNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                _fcNameLabel.style.fontSize = 24;
                _fcNameLabel.style.marginTop = 10;
                _fcNameLabel.style.marginLeft = 10;
                _fcNameLabel.style.position = Position.Absolute;
            }

            EnsureConfigAssetInProject(); // Since it can get nulled out during assembly reload

            CreateModules();
            void CreateModules()
            {
                #region Graphics-rendering
                _gridRenderer = new GridRendererUitk(_fcContext, Config.GridDrawConfig);
                _blockRenderer = new BlockRendererUitk(_fcContext, _blockDrawer);
                #endregion

                #region Viewport-handling
                _panHandler = new PanHandlerUitk(_fcContext);
                _zoomHandler = new ZoomHandlerUitk(_fcContext, Config.MinZoom, Config.MaxZoom);
                _scrollPosResetter = new ScrollPosResetter(_fcContext);
                #endregion

                _blockClickSelectionSyncer = new SingleClickBlockSelector(_fcContext);
                _repaintTriggerer = new FcWindowRepaintTriggerer();
            }

            RegisterModules();
            void RegisterModules()
            {
                #region Graphics-rendering
                _moduleDispatcher.AddModule(_gridRenderer);
                _moduleDispatcher.AddModule(_blockRenderer);
                #endregion

                #region Viewport-handling
                _moduleDispatcher.AddModule(_panHandler);
                _moduleDispatcher.AddModule(_zoomHandler);
                #endregion

                _moduleDispatcher.AddModule(_blockClickSelectionSyncer);
                _moduleDispatcher.AddModule(_repaintTriggerer);
            }

            AttachUiElements();
            void AttachUiElements()
            {
                root.Add(_gridRenderer);
                root.Add(_blockRenderer);
                root.Add(_fcNameLabel);
            }

            InitSubmodules();
            void InitSubmodules()
            {
                _gridRenderer.Initialize(this);
                _panHandler.Initialize(this);
                _blockRenderer.Initialize(this);
                _scrollPosResetter.Initialize(this);
                _blockClickSelectionSyncer.Initialize(this);
                _repaintTriggerer.Initialize(this);
                _zoomHandler.Initialize(this);
            }
            
            FlowchartWindowSignals.ChangedFlowchart(null, _fcContext.Flowchart);
        }

        private Flowchart ActiveFlowchart => EditorSelectionTracker.ActiveFlowchart;
        private MissingFlowchartOverlay _missingOverlay;
        private UitkLabel _fcNameLabel;

        #region Submodules
        private GridRendererUitk _gridRenderer;
        private PanHandlerUitk _panHandler;
        private readonly InputSignalModuleUitk _inputDetector = new InputSignalModuleUitk();
        private BlockInspectorSynchronization _blockInspectorSync;
        private BlockRendererUitk _blockRenderer;
        private SingleClickBlockSelector _blockClickSelectionSyncer;
        private FcWindowRepaintTriggerer _repaintTriggerer;
        private ZoomHandlerUitk _zoomHandler;
        #endregion

        static readonly DefaultBlockDrawerUitk _blockDrawer = new DefaultBlockDrawerUitk();
        private MissingFlowchartOverlay MissingOverlay
        {
            get
            {
                if (_missingOverlay == null)
                {
                    _missingOverlay = new MissingFlowchartOverlay(OnRefreshButtonClicked);
                }

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

        private ScrollPosResetter _scrollPosResetter;

        private FlowchartContext _fcContext;

        private void OnGUI()
        {
            bool inValidState = _fcContext != null && _fcContext.Flowchart != null;
            if (!inValidState)
            {
                return;
            }
            _inputDetector?.OnGUI(Event.current);
            _scrollPosResetter?.OnGUI(Event.current);
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EnsureFlowchartForScene();
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
            _gridRenderer?.RefreshNow();
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
            _moduleDispatcher.ClearModules();//
            DisposeSubmodules();
            NullOutSubmodules();

            _fcContext?.Dispose();
            _fcContext = null;

            _missingOverlay?.Dispose();
            _missingOverlay = null;
            _fcNameLabel = null;

            CreateGUI();
        }
    }

}