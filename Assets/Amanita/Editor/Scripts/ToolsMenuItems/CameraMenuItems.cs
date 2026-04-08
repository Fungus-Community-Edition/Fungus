using UnityEditor;
using AtMycelia.Amanita.VScripting.EditorUtils;

namespace AtMycelia.Amanita.EditorUtils
{
    public class CameraMenuItems 
    {
        [MenuItem("Tools/Amanita/Create/View", false, 100)]
        static void CreateView()
        {
            FlowchartMenuItems.SpawnPrefab("View");
        }
    }
}