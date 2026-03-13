using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    internal sealed class FlowchartWindowTeardownCoordinator
    {
        public FlowchartWindowTeardownResult Teardown(FlowchartWindowTeardownRequest request)
        {
            request.ModuleHost.ClearModules();

            request.FlowchartContext?.Dispose();

            request.GraphicsRenderer?.Dispose();
            request.ViewportManager?.Dispose();

            request.InputDetector.Dispose();
            request.ContextMenuManager?.Dispose();
            request.VariablesPanel?.Dispose();

            request.FcNameLabel?.RemoveFromHierarchy();
            request.MissingOverlay?.Dispose();

            return new FlowchartWindowTeardownResult(
                null,
                null,
                null,
                null,
                null,
                null);
        }
    }

    internal sealed class FlowchartWindowTeardownRequest
    {
        public FlowchartWindowTeardownRequest(
            FlowchartWindowModuleHost moduleHost,
            FlowchartContext flowchartContext,
            FcWindowGraphicsRenderer graphicsRenderer,
            MainViewportManager viewportManager,
            InputSignalModule inputDetector,
            ContextMenuManager contextMenuManager,
            FcWindowVariablesPanel variablesPanel,
            UitkLabel fcNameLabel,
            MissingFlowchartOverlay missingOverlay)
        {
            ModuleHost = moduleHost;
            FlowchartContext = flowchartContext;
            GraphicsRenderer = graphicsRenderer;
            ViewportManager = viewportManager;
            InputDetector = inputDetector;
            ContextMenuManager = contextMenuManager;
            VariablesPanel = variablesPanel;
            FcNameLabel = fcNameLabel;
            MissingOverlay = missingOverlay;
        }

        public FlowchartWindowModuleHost ModuleHost { get; }
        public FlowchartContext FlowchartContext { get; }
        public FcWindowGraphicsRenderer GraphicsRenderer { get; }
        public MainViewportManager ViewportManager { get; }
        public InputSignalModule InputDetector { get; }
        public ContextMenuManager ContextMenuManager { get; }
        public FcWindowVariablesPanel VariablesPanel { get; }
        public UitkLabel FcNameLabel { get; }
        public MissingFlowchartOverlay MissingOverlay { get; }
    }

    internal sealed class FlowchartWindowTeardownResult
    {
        public FlowchartWindowTeardownResult(
            FlowchartContext flowchartContext,
            FcWindowGraphicsRenderer graphicsRenderer,
            MainViewportManager viewportManager,
            ContextMenuManager contextMenuManager,
            FcWindowVariablesPanel variablesPanel,
            UitkLabel fcNameLabel)
        {
            FlowchartContext = flowchartContext;
            GraphicsRenderer = graphicsRenderer;
            ViewportManager = viewportManager;
            ContextMenuManager = contextMenuManager;
            VariablesPanel = variablesPanel;
            FcNameLabel = fcNameLabel;
        }

        public FlowchartContext FlowchartContext { get; }
        public FcWindowGraphicsRenderer GraphicsRenderer { get; }
        public MainViewportManager ViewportManager { get; }
        public ContextMenuManager ContextMenuManager { get; }
        public FcWindowVariablesPanel VariablesPanel { get; }
        public UitkLabel FcNameLabel { get; }
    }
}