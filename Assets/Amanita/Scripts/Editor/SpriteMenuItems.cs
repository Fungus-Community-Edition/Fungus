using UnityEditor;
using Amanita.VScripting.EditorUtils;

namespace Amanita.EditorUtils
{
    public class SpriteMenuItems 
    {
        [MenuItem("Tools/Amanita/Create/Clickable Sprite", false, 150)]
        static void CreateClickableSprite()
        {
            FlowchartMenuItems.SpawnPrefab("ClickableSprite");
        }

        [MenuItem("Tools/Amanita/Create/Draggable Sprite", false, 151)]
        static void CreateDraggableSprite()
        {
            FlowchartMenuItems.SpawnPrefab("DraggableSprite");
        }

        [MenuItem("Tools/Amanita/Create/Drag Target Sprite", false, 152)]
        static void CreateDragTargetSprite()
        {
            FlowchartMenuItems.SpawnPrefab("DragTargetSprite");
        }

        [MenuItem("Tools/Amanita/Create/Parallax Sprite", false, 152)]
        static void CreateParallaxSprite()
        {
            FlowchartMenuItems.SpawnPrefab("ParallaxSprite");
        }
    }
}