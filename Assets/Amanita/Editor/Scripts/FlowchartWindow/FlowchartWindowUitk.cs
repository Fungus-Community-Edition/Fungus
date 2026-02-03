using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

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
            static void EnsureConfigAssetInProject()
            {
                _config = SOUtils.EnsureSOExists<FlowchartWindowConfig>(
                    _configSubfolderPath,
                    _configAssetName);
            }
            FlowchartWindowUitk wnd = _s != null ?
                _s :
                GetWindow<FlowchartWindowUitk>();
            wnd.titleContent = new GUIContent(_config.FlowchartWindowTitle);
            wnd.minSize = _config.WindowMinSize;
        }

        private static FlowchartWindowConfig _config;
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

                FlowchartWindowSignals.LeftClicked += _moduleDispatcher.NotifyLeftClick;
                FlowchartWindowSignals.RightClicked += _moduleDispatcher.NotifyRightClick;
                FlowchartWindowSignals.DoubleClicked += _moduleDispatcher.NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved += _moduleDispatcher.NotifyScrollWheelMoved;
                FlowchartWindowSignals.ScrollWheelDragged += _moduleDispatcher.NotifyScrollWheelDragged;
                FlowchartWindowSignals.EmptySpaceClicked += _moduleDispatcher.NotifyEmptySpaceClicked;
                FlowchartWindowSignals.ChangedFlowchart += _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.BlocksCopied += _moduleDispatcher.NotifyBlocksCopied;
                FlowchartWindowSignals.PreBlockDeletion += _moduleDispatcher.NotifyPreBlockDeletion;
                FlowchartWindowSignals.BlockSelected += _moduleDispatcher.NotifyBlockSelected;
                FlowchartWindowSignals.CommandSelected += _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.WindowPanned += _moduleDispatcher.NotifyWindowPanned;

                EditorSceneManager.sceneOpened += OnSceneOpened;
            }
            else
            {
                EditorSelectionTracker.SelectedFlowchartChanged -= OnSelectedFlowchartChanged;

                FlowchartWindowSignals.LeftClicked -= _moduleDispatcher.NotifyLeftClick;
                FlowchartWindowSignals.RightClicked -= _moduleDispatcher.NotifyRightClick;
                FlowchartWindowSignals.DoubleClicked -= _moduleDispatcher.NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved -= _moduleDispatcher.NotifyScrollWheelMoved;
                FlowchartWindowSignals.ScrollWheelDragged -= _moduleDispatcher.NotifyScrollWheelDragged;
                FlowchartWindowSignals.EmptySpaceClicked -= _moduleDispatcher.NotifyEmptySpaceClicked;
                FlowchartWindowSignals.ChangedFlowchart -= _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.BlocksCopied -= _moduleDispatcher.NotifyBlocksCopied;
                FlowchartWindowSignals.PreBlockDeletion -= _moduleDispatcher.NotifyPreBlockDeletion;
                FlowchartWindowSignals.BlockSelected -= _moduleDispatcher.NotifyBlockSelected;
                FlowchartWindowSignals.CommandSelected -= _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.WindowPanned -= _moduleDispatcher.NotifyWindowPanned;

                EditorSceneManager.sceneOpened -= OnSceneOpened;

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
            UpdateBlockCollection();
            _gridRenderer?.RefreshNow();
            FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
        }

        protected virtual void OnDisable()
        {
            Debug.Log("FlowchartWindowUitk OnDisable");
            ToggleSubs(false);
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(_s, this))
            {
                _s = null;
            }

            _moduleDispatcher.ClearModules();
            _fcContext?.Dispose();
            _fcContext = null;

            DisposeSubmodules();
            NullOutSubmodules();

            _fcNameLabel?.RemoveFromHierarchy();
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
            _selectionSyncer = null;
            _inspectorSync = null;
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

            // If ammy state is missing, we cannot proceed. Show a label and return.
            if (ActiveFlowchart == null)
            {
                PrepErrorLabel(root);
                PrepRefreshButton(root);
                return;
            }

            PrepFcContext();
            void PrepFcContext()
            {
                _fcContext = new FlowchartContext();
                _fcContext.Flowchart = ActiveFlowchart;
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

            PrepSubmodules();
            void PrepSubmodules()
            {
                #region Graphics-rendering modules
                _gridRenderer = new GridRendererUitk(_fcContext, _config.GridDrawConfig);
                _blockRenderer = new BlockRendererUitk(_fcContext, new DefaultBlockDrawerUitk());
                #endregion

                #region For handling the viewport
                _panHandler = new PanHandlerUitk(_fcContext);
                _zoomHandler = new ZoomHandlerUitk(_fcContext, _config.MinZoom, _config.MaxZoom);
                _scrollPosResetter = new ScrollPosResetter(_fcContext);
                #endregion

                _selectionSyncer = new FlowchartSelectionSyncerUitk(_fcContext);
                _inspectorSync = new FcWindowSelectionSyncUitk(_fcContext);

                _moduleDispatcher.AddModule(_gridRenderer);
                _moduleDispatcher.AddModule(_panHandler);
                _moduleDispatcher.AddModule(_blockRenderer);
                _moduleDispatcher.AddModule(_selectionSyncer);
                _moduleDispatcher.AddModule(_inspectorSync);
                _moduleDispatcher.AddModule(_zoomHandler);
            }

            AttachUiElements();
            void AttachUiElements()
            {
                root.Add(_gridRenderer);
                root.Add(_blockRenderer);
                root.Add(_fcNameLabel);
            }

            // TODO: register ui elements in instance fields for further manipulation
            _gridRenderer.RefreshNow();
            _panHandler.Initialize(this);
            _blockRenderer.Initialize(this);
            _scrollPosResetter.Initialize(this);
            _selectionSyncer.Initialize(this);
            _inspectorSync.Initialize(this);
            _zoomHandler.Initialize(this);
            FlowchartWindowSignals.ChangedFlowchart(null, _fcContext.Flowchart);
        }

        private Flowchart ActiveFlowchart => EditorSelectionTracker.ActiveFlowchart;
        private UitkLabel _errorLabel;
        private Button _refreshButton;

        private GridRendererUitk _gridRenderer;
        private PanHandlerUitk _panHandler;
        private readonly InputSignalModuleUitk _inputDetector = new InputSignalModuleUitk();
        private BlockInspectorSynchronization _blockInspectorSync;
        private BlockRendererUitk _blockRenderer;
        private FlowchartSelectionSyncerUitk _selectionSyncer;
        private FcWindowSelectionSyncUitk _inspectorSync;
        private ZoomHandlerUitk _zoomHandler;

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

        private UitkLabel _fcNameLabel;

        void PrepErrorLabel(VisualElement root)
        {
            _errorLabel = new UitkLabel("No Flowcharts found in the scene. ");
            _errorLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _errorLabel.style.fontSize = 48;
            _errorLabel.style.color = Color.yellow;
            // ^ The existence of Flowcharts implies that of AmanitaState.
            root.Add(_errorLabel);
        }

        void PrepRefreshButton(VisualElement root)
        {
            _refreshButton = new Button(OnRefreshButtonClicked);
            _refreshButton.text = "Refresh";
            _refreshButton.style.alignSelf = Align.Center;
            Vector2 buttonSize = new Vector2(200, 50);
            _refreshButton.style.width = buttonSize.x;
            _refreshButton.style.height = buttonSize.y;
            _refreshButton.style.fontSize = 24;
            root.Add(_refreshButton);
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
                RemoveErrorScreenControls();
                CreateGUI();
            }
            else
            {
                Debug.LogWarning("Flowchart still not found on refresh.");
            }

        }

        private ScrollPosResetter _scrollPosResetter;

        void RemoveErrorScreenControls()
        {
            if (_errorLabel == null && _refreshButton == null)
            {
                return;
            }
            VisualElement root = rootVisualElement;
            root.Remove(_errorLabel);
            root.Remove(_refreshButton);
            _errorLabel = null;
            _refreshButton = null;
        }

        internal void UpdateBlockCollection()
        {
            if (_fcContext == null)
            {
                return;
            }

            _blockRenderer?.RefreshBlocks();
        }

        private FlowchartContext _fcContext;

        private void OnGUI()
        {
            _inputDetector.OnGUI(Event.current);
            _scrollPosResetter.OnGUI(Event.current);
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
                ShowMissingFlowchartUi();
                return;
            }

            RemoveErrorScreenControls();
            Flowchart previous = _fcContext.Flowchart;
            _fcContext.Flowchart = fallback;
            UpdateBlockCollection();
            _gridRenderer?.RefreshNow();
            if (!ReferenceEquals(previous, fallback))
            {
                FlowchartWindowSignals.ChangedFlowchart(previous, fallback);
            }
        }

        private void ShowMissingFlowchartUi()
        {
            VisualElement root = rootVisualElement;
            if (_errorLabel == null)
            {
                PrepErrorLabel(root);
            }

            if (_refreshButton == null)
            {
                PrepRefreshButton(root);
            }
        }
    }

}