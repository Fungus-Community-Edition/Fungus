using UnityEditor;
using Amanita.VScripting.EditorUtils;

namespace Amanita.EditorUtils
{
    public class FcWindowFocusChecker : IFocusChecker
    {
        public bool CheckFocus(FlowchartContext ctx)
        {
            return EditorWindow.focusedWindow is IFlowchartHost;
        }
    }
}