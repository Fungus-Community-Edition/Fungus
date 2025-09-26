using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class FcWindowZoomPanComponent : IFcWindowComponent
    {
        private IFlowchartHost host;

        public virtual void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(IFlowchartHost hostWindow)
        {
            host = hostWindow;
        }

        /// <summary>
        /// Call from FlowchartWindow.OnGUI() in the toolbar area.
        /// </summary>
        public void OnGUI()
        {
            // Label + slider
            GUILayout.Label("Scale", EditorStyles.miniLabel);

            float oldZoom = host.Flowchart.Zoom;
            float newZoom = GUILayout.HorizontalSlider(
                oldZoom,
                FlowchartWindow.MinZoomValue,
                FlowchartWindow.MaxZoomValue,
                GUILayout.MinWidth(40),
                GUILayout.MaxWidth(100)
            );

            // If changed, apply delta centered on window
            if (!Mathf.Approximately(newZoom, oldZoom))
            {
                float delta = newZoom - oldZoom;
                // Zoom around window center
                host.DoZoom(delta, Vector2.one * 0.5f);
            }

            // Current zoom factor text
            GUILayout.Label(host.Flowchart.Zoom.ToString("0.0#x"),
                EditorStyles.miniLabel,
                GUILayout.Width(30)
            );

            // Optional: Center button
            if (GUILayout.Button("Center", EditorStyles.toolbarButton))
                host.CenterFlowchart();
        }

        // No-op for other hooks
        public void OnGUI(DrawBlockContext ctx, FlowchartContext fctx) { }
        public void OnEditorUpdate() { }
        public void OnInspectorUpdate() { }

        public void OnToolbarGUI()
        {

        }

        public void OnInspectorGUI()
        {

        }
    }
}