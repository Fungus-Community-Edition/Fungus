using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    internal sealed class FcwUiBuilder
    {
        public FcwUiBuildResult Build(FcwUiBuildRequest request)
        {
            VisualElement uxmlRoot = request.VisualTreeAsset.Instantiate();

            PrepRoot();
            void PrepRoot()
            {
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
            }

            if (request.ActiveFlowchart == null)
            {
                request.MissingOverlay.Show(uxmlRoot);
                return FcwUiBuildResult.Missing(uxmlRoot);
            }

            request.MissingOverlay.Hide();

            AmanitaClipboard clipboard = request.Clipboard ?? new AmanitaClipboard(request.FlowchartHost);

            FlowchartContext context;
            PrepFcContext();
            void PrepFcContext()
            {
                context = new FlowchartContext();
                context.Flowchart = request.ActiveFlowchart;
                if (context.Flowchart == null)
                {
                    context.Flowchart = UnityObj.FindFirstObjectByType<Flowchart>();
                    Debug.Log($"Going with first Flowchart found in the project: {context.Flowchart.name}");
                }
                context.FcHost = request.FlowchartHost;
                context.Position = new Rect(0, 0, request.WindowRect.width, request.WindowRect.height);
                context.GridObjectSnap = 10f;
            }

            UitkLabel fcNameLabel = uxmlRoot.Q<UitkLabel>("FcNameLabel");
            UitkLabel zoomLabel = uxmlRoot.Q<UitkLabel>("ZoomLabel");
            UpdateLabels(context, fcNameLabel, zoomLabel);

            FcwGraphicsRenderer graphicsRenderer = new FcwGraphicsRenderer(context, request.Config.GridDrawConfig,
                request.BlockDrawer);
            MainViewportManager viewportManager = new MainViewportManager(context, request.Config.MinZoom,
                request.Config.MaxZoom);

            ContextMenuManager contextMenuManager = new ContextMenuManager();
            FcwVariablesPanel variablesPanel = new FcwVariablesPanel();

            RegisterInModuleHost();
            void RegisterInModuleHost()
            {
                var host = request.ModuleHost;
                host.Register(graphicsRenderer);
                host.Register(viewportManager);

                host.Register(contextMenuManager);
                host.Register(request.InputDetector);
                host.Register(variablesPanel);
            }

            uxmlRoot.Add(graphicsRenderer);
            uxmlRoot.Add(fcNameLabel);

            graphicsRenderer.Initialize(request.FlowchartHost as FlowchartWindow);
            viewportManager.Initialize(request.FlowchartHost as FlowchartWindow);

            request.InputDetector.Initialize(request.FlowchartHost as FlowchartWindow);
            contextMenuManager.Initialize(request.FlowchartHost as FlowchartWindow);
            variablesPanel.Initialize(request.FlowchartHost as FlowchartWindow);

            return new FcwUiBuildResult(uxmlRoot, true,
                clipboard, context,
                fcNameLabel, zoomLabel,
                graphicsRenderer, viewportManager,
                contextMenuManager, variablesPanel);
        }

        private void UpdateLabels(FlowchartContext context, UitkLabel fcNameLabel, UitkLabel zoomAmountLabel)
        {
            if (context == null || context.Flowchart == null)
            {
                fcNameLabel.text = zoomAmountLabel.text = string.Empty;
                return;
            }

            if (fcNameLabel != null)
            {
                fcNameLabel.text = $"FC: {context.Flowchart.name}";
            }

            if (zoomAmountLabel != null)
            {
                float zoomAmount = context.Flowchart.Zoom;
                string amountToShow = Mathf.Round(zoomAmount * 100).ToString();
                zoomAmountLabel.text = $"Zoom: {amountToShow}%";
            }
        }
    }

    internal sealed class FcwUiBuildRequest
    {
        public FcwUiBuildRequest(
            VisualElement rootVisualElement,
            VisualTreeAsset visualTreeAsset,
            Flowchart activeFlowchart,
            MissingFlowchartOverlay missingOverlay,
            AmanitaClipboard clipboard,
            FlowchartWindowConfig config,
            DefaultBlockDrawer blockDrawer,
            IFlowchartHostCore flowchartHost,
            Rect windowRect,
            FcwModuleHost moduleHost,
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
        public FcwModuleHost ModuleHost { get; }
        public InputSignalModule InputDetector { get; }
    }

    internal sealed class FcwUiBuildResult
    {
        public FcwUiBuildResult(
            VisualElement uxmlRoot,
            bool hasFlowchart,
            AmanitaClipboard clipboard,
            FlowchartContext flowchartContext,
            UitkLabel fcNameLabel,
            UitkLabel zoomLabel,
            FcwGraphicsRenderer graphicsRenderer,
            MainViewportManager viewportManager,
            ContextMenuManager contextMenuManager,
            FcwVariablesPanel variablesPanel)
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
        public FcwGraphicsRenderer GraphicsRenderer { get; }
        public MainViewportManager ViewportManager { get; }
        public ContextMenuManager ContextMenuManager { get; }
        public FcwVariablesPanel VariablesPanel { get; }

        public static FcwUiBuildResult Missing(VisualElement uxmlRoot)
        {
            return new FcwUiBuildResult(
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