using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartWindowUitk : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/Atelier Mycelia/Experimental/FlowchartWindowUitk")]
        public static void ShowFromMenuItem()
        {
            FlowchartWindowUitk wnd = GetWindow<FlowchartWindowUitk>();
            wnd.titleContent = new GUIContent("FlowchartWindowUitk");
            wnd.minSize = fcWindowMinSize;
        }

        private static readonly Vector2 fcWindowMinSize = new Vector2(800, 500);

        private void OnEnable()
        {
            _ammyState = FindFirstObjectByType<AmanitaState>();
            ToggleSubs(true);
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                _ammyState.SelectedFlowchartChanged += OnSelectedFlowchartChanged;

                FlowchartWindowSignals.LeftClicked += _moduleDispatcher.NotifyLeftClick;
                FlowchartWindowSignals.RightClicked += _moduleDispatcher.NotifyRightClick;
                FlowchartWindowSignals.DoubleClicked += _moduleDispatcher.NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved += _moduleDispatcher.NotifyScrollWheelMoved;
                FlowchartWindowSignals.EmptySpaceClicked += _moduleDispatcher.NotifyEmptySpaceClicked;
                FlowchartWindowSignals.ChangedFlowchart += _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.BlocksCopied += _moduleDispatcher.NotifyBlocksCopied;
                FlowchartWindowSignals.PreBlockDeletion += _moduleDispatcher.NotifyPreBlockDeletion;
                FlowchartWindowSignals.BlockSelected += _moduleDispatcher.NotifyBlockSelected;
                FlowchartWindowSignals.CommandSelected += _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.WindowPanned += _moduleDispatcher.NotifyWindowPanned;
            }
            else
            {
                _ammyState.SelectedFlowchartChanged -= OnSelectedFlowchartChanged;

                FlowchartWindowSignals.LeftClicked -= _moduleDispatcher.NotifyLeftClick;
                FlowchartWindowSignals.RightClicked -= _moduleDispatcher.NotifyRightClick;
                FlowchartWindowSignals.DoubleClicked -= _moduleDispatcher.NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved -= _moduleDispatcher.NotifyScrollWheelMoved;
                FlowchartWindowSignals.EmptySpaceClicked -= _moduleDispatcher.NotifyEmptySpaceClicked;
                FlowchartWindowSignals.ChangedFlowchart -= _moduleDispatcher.NotifyFlowchartChanged;
                FlowchartWindowSignals.BlocksCopied -= _moduleDispatcher.NotifyBlocksCopied;
                FlowchartWindowSignals.PreBlockDeletion -= _moduleDispatcher.NotifyPreBlockDeletion;
                FlowchartWindowSignals.BlockSelected -= _moduleDispatcher.NotifyBlockSelected;
                FlowchartWindowSignals.CommandSelected -= _moduleDispatcher.NotifyCommandSelected;
                FlowchartWindowSignals.WindowPanned -= _moduleDispatcher.NotifyWindowPanned;

            }
        }

        private readonly FlowchartModuleDispatcher _moduleDispatcher = new FlowchartModuleDispatcher();

        private void OnSelectedFlowchartChanged(Flowchart flowchart)
        {
            _fcContext.Flowchart = flowchart;
            _gridRenderer.RefreshNow();
        }

        private void OnDisable()
        {
            Debug.Log("FlowchartWindowUitk OnDisable");
            ToggleSubs(false);
        }

        private void OnDestroy()
        {
            _moduleDispatcher.ClearModules();
            _fcContext?.Dispose();
            _fcContext = null;
        }
        
        public void CreateGUI()
        {
            _moduleDispatcher.ClearModules();
            VisualElement root = rootVisualElement;

            PrepFcContext();
            void PrepFcContext()
            {
                _fcContext = new FlowchartContext();
                _fcContext.Flowchart = _ammyState.SelectedFlowchart;
                _fcContext.FcHost = null; // TODO: assign proper host
                _fcContext.Position = new Rect(0, 0, position.width, position.height);
                _fcContext.GridObjectSnap = 10f;
            }
            
            PrepSubmodules();
            void PrepSubmodules()
            {
                _gridRenderer = new GridRendererUitk(_fcContext, _drawGridSettings);
                _panHandler = new PanHandlerUitk(_fcContext);
                // TODO: prepare other submodules
                _moduleDispatcher.AddModule(_gridRenderer);
                _moduleDispatcher.AddModule(_panHandler);
            }

            AttachUiElements();
            void AttachUiElements()
            {
                root.Add(_gridRenderer);
            }

            // TODO: register ui elements in instance fields for further manipulation
            _gridRenderer.RefreshNow();
        }

        private AmanitaState _ammyState;
        private Block _lastSelectedBlock;
        private Command _lastSelectedCommand;
        private GridRendererUitk _gridRenderer;
        private PanHandlerUitk _panHandler;
        private readonly InputSignalModuleUitk _inputDetector = new InputSignalModuleUitk();

        private readonly DrawGridContext _drawGridSettings = new DrawGridContext
        {
            GridLineSpacingSize = 50f,
            GridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.2f)
        };

        private FlowchartContext _fcContext;

        private void OnGUI()
        {
            _inputDetector.OnGUI(Event.current);
        }
    }

}