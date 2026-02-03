using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public static class BlockHitTester
    {
        public static bool IsMouseOverBlock(Vector2 mousePosition)
        {
            Flowchart flowchart = EditorSelectionTracker.ActiveFlowchart;
            if (flowchart == null)
            {
                return false;
            }

            bool zoomAtZero = Mathf.Approximately(flowchart.Zoom, 0f);
            float zoom = zoomAtZero ? 
                1f : 
                flowchart.Zoom;
            Vector2 mousePosInWindowSpace = mousePosition / zoom;
            Vector2 scrollPos = flowchart.ScrollPos;

            IReadOnlyCollection<Block> blocks = flowchart.Blocks;
            if (blocks == null || blocks.Count == 0)
            {
                Block[] fallback = flowchart.GetComponents<Block>();
                for (int i = 0; i < fallback.Length; i++)
                {
                    bool isOverBlock = IsMouseOverBlock(fallback[i], mousePosInWindowSpace, scrollPos);
                    if (isOverBlock)
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (Block block in blocks)
            {
                bool isOverBlock = IsMouseOverBlock(block, mousePosInWindowSpace, scrollPos);
                if (isOverBlock)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMouseOverBlock(Block block, Vector2 mousePosInWindowSpace, Vector2 scrollPos)
        {
            if (block == null)
            {
                return false;
            }

            Rect windowSpaceRect = block._NodeRect;
            windowSpaceRect.position += scrollPos;

            return windowSpaceRect.Contains(mousePosInWindowSpace);
        }
    }
}