using System;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface IFcWindowComponent : IDisposable
    {
        // Called once when the window enables
        void Initialize(IFlowchartViewHost window);

        // Called inside OnGUI before and after zoom‐area, as needed
        void OnToolbarGUI();
        void OnGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx);
        void OnInspectorGUI();

        // Called each editor‐update
        void OnEditorUpdate();

        void OnInspectorUpdate();
    }
}