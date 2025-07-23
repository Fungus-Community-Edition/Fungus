namespace Amanita.EditorUtils
{
    public interface IFcWindowComponent
    {
        // Called once when the window enables
        void Initialize(FlowchartWindow window);

        // Called inside OnGUI before and after zoom‐area, as needed
        void OnToolbarGUI();
        void OnCanvasGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx);
        void OnInspectorGUI();

        // Called each editor‐update
        void OnEditorUpdate();

        void OnInspectorUpdate();
    }
}