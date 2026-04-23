namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Hosts the dispatchers for the Flowchart Window modules. The dispatchers are responsible 
    /// for dispatching events to the modules.
    /// </summary>
    public sealed class FcwModuleHost
    {
        private readonly BlockModuleDispatcher _blockModuleDispatcher = new BlockModuleDispatcher();
        private readonly MouseModuleDispatcher _mouseModuleDispatcher = new MouseModuleDispatcher();
        private readonly FlowchartModuleDispatcher _moduleDispatcher = new FlowchartModuleDispatcher();

        public BlockModuleDispatcher BlockDispatcher => _blockModuleDispatcher;
        public MouseModuleDispatcher MouseDispatcher => _mouseModuleDispatcher;
        public FlowchartModuleDispatcher ModuleDispatcher => _moduleDispatcher;

        public void ToggleDispatcherSubs(bool on)
        {
            _blockModuleDispatcher.ToggleSubs(on);
            _mouseModuleDispatcher.ToggleSubs(on);
        }

        public void Register(IFlowchartWindowModule module)
        {
            if (module == null)
            {
                return;
            }

            _moduleDispatcher.AddModule(module);
            _blockModuleDispatcher.AddModule(module);
            _mouseModuleDispatcher.AddModule(module);
        }

        public void ClearModules()
        {
            _blockModuleDispatcher.ClearModules();
            _mouseModuleDispatcher.ClearModules();
            _moduleDispatcher.ClearModules();
        }
    }
}