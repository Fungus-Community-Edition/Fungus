using UnityEditor;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class FcWindowFocusChecker : IFocusChecker
    {
        public bool CheckFocus(FlowchartContext ctx)
        {
            return EditorWindow.focusedWindow is IFlowchartHost;
        }
    }
}