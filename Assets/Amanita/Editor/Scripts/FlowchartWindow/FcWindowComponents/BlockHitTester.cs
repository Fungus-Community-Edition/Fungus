using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            IReadOnlyCollection<Block> blocks = flowchart.Blocks;
            if (blocks == null || blocks.Count == 0)
            {
                Block[] fallback = flowchart.GetComponents<Block>();
                for (int i = 0; i < fallback.Length; i++)
                {
                    bool isOverBlock = IsMouseOverBlock(fallback[i], flowchart, mousePosition);
                    if (isOverBlock)
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (Block block in blocks)
            {
                bool isOverBlock = IsMouseOverBlock(block, flowchart, mousePosition);
                if (isOverBlock)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetBlockWindowRect(Block block, Flowchart flowchart, out Rect rect)
        {
            rect = default;
            if (block == null || flowchart == null)
            {
                return false;
            }

            if (TryGetBlockRectFromRenderer(block, out rect))
            {
                return true;
            }

            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 1f : flowchart.Zoom;
            Vector2 scrollPos = flowchart.ScrollPos;

            rect = block._NodeRect;
            rect.position = (rect.position + scrollPos) * zoom;
            rect.size *= zoom;

            return true;
        }

        private static bool IsMouseOverBlock(Block block, Flowchart flowchart, Vector2 mousePosition)
        {
            if (!TryGetBlockWindowRect(block, flowchart, out Rect windowSpaceRect))
            {
                return false;
            }

            return windowSpaceRect.Contains(mousePosition);
        }

        private static bool TryGetBlockRectFromRenderer(Block block, out Rect rect)
        {
            rect = default;

            FlowchartWindowUitk window = FlowchartWindowUitk.S;
            if (window == null)
            {
                return false;
            }

            VisualElement root = window.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            BlockRendererUitk renderer = root.Q<BlockRendererUitk>();
            if (renderer == null || !renderer.TryGetBlockRect(block, out Rect localRect))
            {
                return false;
            }

            VisualElement parent = renderer.parent;
            if (parent == null)
            {
                rect = localRect;
                return true;
            }

            Vector2 worldPos = parent.LocalToWorld(localRect.position);
            rect = new Rect(worldPos, localRect.size);
            return true;
        }
    }
}