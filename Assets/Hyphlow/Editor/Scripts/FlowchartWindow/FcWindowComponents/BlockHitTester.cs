using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public static class BlockHitTester
    {
        public static bool IsMouseOverBlock(Vector2 mousePosition)
        {
            if (ActiveFlowchart == null)
            {
                return false;
            }

            IReadOnlyCollection<Block> blocks = ActiveFlowchart.Blocks;
            if (blocks == null || blocks.Count == 0)
            {
                Block[] fallback = ActiveFlowchart.GetComponents<Block>();
                for (int i = 0; i < fallback.Length; i++)
                {
                    bool isOverBlock = IsMouseOverBlock(fallback[i], ActiveFlowchart, mousePosition);
                    if (isOverBlock)
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (Block block in blocks)
            {
                bool isOverBlock = IsMouseOverBlock(block, ActiveFlowchart, mousePosition);
                if (isOverBlock)
                {
                    return true;
                }
            }

            return false;
        }

        private static Flowchart ActiveFlowchart
        {
            get
            {
                Flowchart result = EditorSelectionTracker.ActiveFlowchart;
                if (result == null)
                {
                    // When the FlowchartWindow is docked, ActiveFlowchart might be null because the window's
                    // context may not be fully initialized. In that case, we can attempt to get the _last_
                    // active Flowchart as a fallback, which should still allow block hit testing to work in most cases.
                    result = EditorSelectionTracker.LastActiveFlowchart;
                }

                return result;
                
            }
        }

        /// <summary>
        /// Tries to get the block's rect in window space. It first attempts to get the rect 
        /// from the BlockRendererUitk for better accuracy, and falls back to calculating it 
        /// from the block's NodeRect if necessary.
        /// </summary>
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

            FlowchartWindow window = FlowchartWindow.S;
            if (window == null)
            {
                return false;
            }

            VisualElement root = window.rootVisualElement;
            if (root == null)
            {
                return false;
            }

            BlockRenderer renderer = root.Q<BlockRenderer>();
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

        public static Rect ToFlowchartSpace(Rect selectionBox, Flowchart flowchart)
        {
            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ?
                1f :
                flowchart.Zoom;

            Rect zoomSelectionBox = selectionBox;
            zoomSelectionBox.position -= flowchart.ScrollPos * zoom;
            zoomSelectionBox.position /= zoom;
            zoomSelectionBox.size /= zoom;

            return zoomSelectionBox;
        }

        public static Vector2 ToWorldPosition(Vector2 localPosition, VisualElement hasLocalSpace)
        {
            var worldTrans = hasLocalSpace.worldTransform;
            Vector3 world = worldTrans.MultiplyPoint3x4(localPosition);
            return new Vector2(world.x, world.y);
        }

        public static Block FindTopmostBlock(Vector2 mousePosition)
        {
            if (ActiveFlowchart == null)
            {
                return null;
            }

            var blocks = ActiveFlowchart.Blocks;
            if (blocks == null || blocks.Count == 0)
            {
                return null;
            }

            Block topmost = null;
            foreach (var blockEl in blocks)
            {
                if (blockEl == null)
                {
                    continue;
                }

                if (TryGetBlockWindowRect(blockEl, ActiveFlowchart, out Rect rect) &&
                    rect.Contains(mousePosition))
                {
                    topmost = blockEl;
                }
            }

            return topmost;
        }
    }
}