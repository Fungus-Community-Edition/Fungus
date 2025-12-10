using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class FcWindowContextMenuComponent : IFcWindowComponent
    {
        private IFlowchartHost host;
        private Vector2 rightClickDown;

        public void Initialize(IFlowchartHost hostWindow)
        {
            host = hostWindow;
        }

        protected BlockContextMenuHandler menuHandler;

        public void OnGUI()
        {
            var e = Event.current;

            // Record right-mouse down position
            if (e.type == EventType.MouseDown && e.button == 1)
                rightClickDown = e.mousePosition;

            // On mouse up, if it’s a click (not a drag), show menu
            if (e.type == EventType.MouseUp && e.button == 1)
            {
                if (Vector2.Distance(rightClickDown, e.mousePosition) < 4f)
                {
                    ShowContextMenu(e.mousePosition);
                    e.Use();
                }
            }
        }

        private void ShowContextMenu(Vector2 pos)
        {
            var menu = new GenericMenu();
            // 1) Paste
            if (host.HasClipboard)
                menu.AddItem(new GUIContent("Paste"), false,
                    () => host.Clipboard.Paste(host.CalcFlowchartWindowViewRect().PointToNormalized(pos)));
            else
                menu.AddDisabledItem(new GUIContent("Paste"));

            // 2) Create Block
            menu.AddItem(new GUIContent("Create Block"), false,
                () => host.CreateBlock(host.Flowchart, host.CalcFlowchartWindowViewRect().PointToNormalized(pos)));

            menu.ShowAsContext();
        }

        // No-ops for other hooks
        public void OnCanvasGUI(DrawBlockContext ctx, FlowchartContext fctx) { }
        public void OnEditorUpdate() { }
        public void OnInspectorUpdate() { }

        public void OnToolbarGUI()
        {
        }

        public void OnGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx)
        {
        }

        public void OnInspectorGUI()
        {
        }

        public virtual void Dispose()
        {
            host = null;
            rightClickDown = Vector2.zero;
        }
    }

    // Extension to convert window-space to flowchart-space
    static class RectExtensions
    {
        public static Vector2 PointToNormalized(this Rect viewRect, Vector2 windowPoint)
        {
            // Inverse of Zoom + Scroll
            return (windowPoint / viewRect.size) * viewRect.size - viewRect.position;
        }
    }
}