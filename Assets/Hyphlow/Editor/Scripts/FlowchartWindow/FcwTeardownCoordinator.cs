using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    internal sealed class FcwTeardownCoordinator
    {
        public FcwTeardownResult Teardown(FcwTeardownRequest request)
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

            return new FcwTeardownResult(
                null,
                null,
                null,
                null,
                null,
                null);
        }
    }

    internal sealed class FcwTeardownRequest
    {
        public FcwTeardownRequest(
            FcwModuleHost moduleHost,
            FlowchartContext flowchartContext,
            FcwGraphicsRenderer graphicsRenderer,
            MainViewportManager viewportManager,
            InputSignalModule inputDetector,
            ContextMenuManager contextMenuManager,
            FcwVariablesPanel variablesPanel,
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

        public FcwModuleHost ModuleHost { get; }
        public FlowchartContext FlowchartContext { get; }
        public FcwGraphicsRenderer GraphicsRenderer { get; }
        public MainViewportManager ViewportManager { get; }
        public InputSignalModule InputDetector { get; }
        public ContextMenuManager ContextMenuManager { get; }
        public FcwVariablesPanel VariablesPanel { get; }
        public UitkLabel FcNameLabel { get; }
        public MissingFlowchartOverlay MissingOverlay { get; }
    }

    internal sealed class FcwTeardownResult
    {
        public FcwTeardownResult(
            FlowchartContext flowchartContext,
            FcwGraphicsRenderer graphicsRenderer,
            MainViewportManager viewportManager,
            ContextMenuManager contextMenuManager,
            FcwVariablesPanel variablesPanel,
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
        public FcwGraphicsRenderer GraphicsRenderer { get; }
        public MainViewportManager ViewportManager { get; }
        public ContextMenuManager ContextMenuManager { get; }
        public FcwVariablesPanel VariablesPanel { get; }
        public UitkLabel FcNameLabel { get; }
    }
}