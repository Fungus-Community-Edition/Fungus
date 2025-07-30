using UnityEditor;
using Amanita.VScripting.EditorUtils;

namespace Amanita.EditorUtils
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