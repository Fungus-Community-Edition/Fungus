using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using AtMycelia.Amanita.EditorUtils;
using System;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    internal sealed class FlowchartWindowUiBuilder
    {
        public FlowchartWindowUiBuildResult Build(FlowchartWindowUiBuildRequest request)
        {
            VisualElement uxmlRoot = request.VisualTreeAsset.Instantiate();
            request.RootVisualElement.Add(uxmlRoot);
            uxmlRoot.pickingMode = PickingMode.Position;
            // ^So that PointerUp events trigger properly when clicking on empty space.
            // Sub-elements can override this to receive events as normal.
            uxmlRoot.SetPadding(0);
            uxmlRoot.SetMargin(0);
            uxmlRoot.style.flexGrow = 1f;
            uxmlRoot.style.width = Length.Percent(100);
            uxmlRoot.style.height = Length.Percent(100);
            // ^To take up the full space of the window

            if (request.ActiveFlowchart == null)
            {
                request.MissingOverlay.Show(uxmlRoot);
                return FlowchartWindowUiBuildResult.Missing(uxmlRoot);
            }

            request.MissingOverlay.Hide();

            AmanitaClipboard clipboard = request.Clipboard ?? new AmanitaClipboard(request.FlowchartHost);

            FlowchartContext context = new FlowchartContext();
            context.Flowchart = request.ActiveFlowchart;
            if (context.Flowchart == null)
            {
                context.Flowchart = UnityEngine.Object.FindFirstObjectByType<Flowchart>();
            }
            context.FcHost = request.FlowchartHost;
            context.Position = new Rect(0, 0, request.WindowRect.width, request.WindowRect.height);
            context.GridObjectSnap = 10f;

            string labelText = "No Flowchart Selected";
            if (context.Flowchart != null)
            {
                labelText = $"FC: {context.Flowchart.name}";
            }

            UitkLabel fcNameLabel = uxmlRoot.Q<UitkLabel>("FcNameLabel");
            fcNameLabel.text = labelText;

            UitkLabel zoomLabel = uxmlRoot.Q<UitkLabel>("ZoomLabel");
            float newZoom = context.Flowchart != null ?
                context.Flowchart.Zoom :
                1f;
            zoomLabel.text = $"Zoom: {Math.Round(newZoom * 100)}%";

            FcWindowGraphicsRenderer graphicsRenderer = new FcWindowGraphicsRenderer(context, request.Config.GridDrawConfig,
                request.BlockDrawer);
            MainViewportManager viewportManager = new MainViewportManager(context, request.Config.MinZoom,
                request.Config.MaxZoom);

            ContextMenuManager contextMenuManager = new ContextMenuManager();
            FcWindowVariablesPanel variablesPanel = new FcWindowVariablesPanel();

            request.ModuleHost.Register(graphicsRenderer);
            request.ModuleHost.Register(viewportManager);

            request.ModuleHost.Register(contextMenuManager);
            request.ModuleHost.Register(request.InputDetector);
            request.ModuleHost.Register(variablesPanel);

            uxmlRoot.Add(graphicsRenderer);
            uxmlRoot.Add(fcNameLabel);

            graphicsRenderer.Initialize(request.FlowchartHost as FlowchartWindow);
            viewportManager.Initialize(request.FlowchartHost as FlowchartWindow);

            request.InputDetector.Initialize(request.FlowchartHost as FlowchartWindow);
            contextMenuManager.Initialize(request.FlowchartHost as FlowchartWindow);
            variablesPanel.Initialize(request.FlowchartHost as FlowchartWindow);

            return new FlowchartWindowUiBuildResult(
                uxmlRoot,
                true,
                clipboard,
                context,
                fcNameLabel,
                zoomLabel,
                graphicsRenderer,
                viewportManager,
                contextMenuManager,
                variablesPanel);
        }
    }

    internal sealed class FlowchartWindowUiBuildRequest
    {
        public FlowchartWindowUiBuildRequest(
            VisualElement rootVisualElement,
            VisualTreeAsset visualTreeAsset,
            Flowchart activeFlowchart,
            MissingFlowchartOverlay missingOverlay,
            AmanitaClipboard clipboard,
            FlowchartWindowConfig config,
            DefaultBlockDrawer blockDrawer,
            IFlowchartHostCore flowchartHost,
            Rect windowRect,
            FlowchartWindowModuleHost moduleHost,
            InputSignalModule inputDetector)
        {
            RootVisualElement = rootVisualElement;
            VisualTreeAsset = visualTreeAsset;
            ActiveFlowchart = activeFlowchart;
            MissingOverlay = missingOverlay;
            Clipboard = clipboard;
            Config = config;
            BlockDrawer = blockDrawer;
            FlowchartHost = flowchartHost;
            WindowRect = windowRect;
            ModuleHost = moduleHost;
            InputDetector = inputDetector;
        }

        public VisualElement RootVisualElement { get; }
        public VisualTreeAsset VisualTreeAsset { get; }
        public Flowchart ActiveFlowchart { get; }
        public MissingFlowchartOverlay MissingOverlay { get; }
        public AmanitaClipboard Clipboard { get; }
        public FlowchartWindowConfig Config { get; }
        public DefaultBlockDrawer BlockDrawer { get; }
        public IFlowchartHostCore FlowchartHost { get; }
        public Rect WindowRect { get; }
        public FlowchartWindowModuleHost ModuleHost { get; }
        public InputSignalModule InputDetector { get; }
    }

    internal sealed class FlowchartWindowUiBuildResult
    {
        public FlowchartWindowUiBuildResult(
            VisualElement uxmlRoot,
            bool hasFlowchart,
            AmanitaClipboard clipboard,
            FlowchartContext flowchartContext,
            UitkLabel fcNameLabel,
            UitkLabel zoomLabel,
            FcWindowGraphicsRenderer graphicsRenderer,
            MainViewportManager viewportManager,
            ContextMenuManager contextMenuManager,
            FcWindowVariablesPanel variablesPanel)
        {
            UxmlRoot = uxmlRoot;
            HasFlowchart = hasFlowchart;
            Clipboard = clipboard;
            FlowchartContext = flowchartContext;
            FcNameLabel = fcNameLabel;
            ZoomLabel = zoomLabel;
            GraphicsRenderer = graphicsRenderer;
            ViewportManager = viewportManager;
            ContextMenuManager = contextMenuManager;
            VariablesPanel = variablesPanel;
        }

        public VisualElement UxmlRoot { get; }
        public bool HasFlowchart { get; }
        public AmanitaClipboard Clipboard { get; }
        public FlowchartContext FlowchartContext { get; }
        public UitkLabel FcNameLabel { get; }
        public UitkLabel ZoomLabel { get; }
        public FcWindowGraphicsRenderer GraphicsRenderer { get; }
        public MainViewportManager ViewportManager { get; }
        public ContextMenuManager ContextMenuManager { get; }
        public FcWindowVariablesPanel VariablesPanel { get; }

        public static FlowchartWindowUiBuildResult Missing(VisualElement uxmlRoot)
        {
            return new FlowchartWindowUiBuildResult(
                uxmlRoot,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }
    }
}