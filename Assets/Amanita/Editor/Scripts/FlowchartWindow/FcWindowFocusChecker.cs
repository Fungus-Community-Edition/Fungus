using UnityEditor;
using AtMycelia.Amanita.VScripting.EditorUtils;

namespace AtMycelia.Amanita.EditorUtils
{
    public class FcWindowFocusChecker : IFocusChecker
    {
        public bool CheckFocus(FlowchartContext ctx)
        {
            return EditorWindow.focusedWindow is IFlowchartHost;
        }
    }
}