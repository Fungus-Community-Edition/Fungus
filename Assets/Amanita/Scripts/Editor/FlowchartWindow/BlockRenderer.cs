using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Handles drawing all the Blocks in the currently-selected Flowchart
    /// </summary>
    public class BlockRenderer 
    {
        public BlockRenderer(IBlockDrawer drawer, IBlockGraphicsGenerator graphicsGenerator)
        {
            _drawer = drawer;
            _graphicsGenerator = graphicsGenerator;
        }

        protected readonly IBlockDrawer _drawer;
        protected readonly IBlockGraphicsGenerator _graphicsGenerator;

        public virtual void Render(DrawBlockContext drawCtx)
        {
            var flowchartCtx = drawCtx.FlowchartCtx;
            var fc = flowchartCtx.Flowchart;
            var viewRect = drawCtx.ViewRect;        // in “world” units (i.e. zoomed & scrolled space)

            for (int i = 0; i < flowchartCtx.AllBlocks.Count; i++)
            {
                var block = flowchartCtx.AllBlocks[i];

                var blockRect = ToWindowSpaceRect(block._NodeRect, drawCtx);

                bool isVisibleOnScreen = viewRect.Overlaps(blockRect);
                if (!isVisibleOnScreen)
                    continue;

                drawCtx.Graphics = _graphicsGenerator.GenerateFor(block);
                _drawer.Draw(block, drawCtx);
            }
        }
    
        protected virtual Rect ToWindowSpaceRect(Rect baseRect, DrawBlockContext drawCtx)
        {
            Flowchart fc = drawCtx.FlowchartCtx.Flowchart;
            Rect result = baseRect;

            if (drawCtx.UseGridSnap)
                result = result.SnapPosition(drawCtx.GridObjectSnap);
            result.position += fc.ScrollPos;

            return result;
        }
    }

    public interface IBlockDrawer
    {
        void Draw(Block toDraw, DrawBlockContext drawCtx);
    }

    public class DefaultBlockDrawer : IBlockDrawer
    {
        public void Draw(Block block, DrawBlockContext drawCtx)
        {
            Debug.Log($"Drawing block {block.BlockName}");

            float blockMinWidth = drawCtx.BlockMinWidth;
            float blockMaxWidth = drawCtx.BlockMaxWidth;
            float defaultBlockHeight = drawCtx.DefaultBlockHeight;
            float gridObjectSnap = drawCtx.GridObjectSnap;

            GUIStyle nodeStyle = drawCtx.NodeStyle,
                descriptionStyle = drawCtx.DescriptionStyle,
                handlerStyle = drawCtx.HandlerStyle;

            Flowchart currentFlowchart = drawCtx.FlowchartCtx.Flowchart;
            Rect scriptViewRect = drawCtx.ViewRect;

            float nodeWidthPadding = 10;
            float nodeWidthA = nodeStyle.CalcSize(new GUIContent(block.BlockName)).x + nodeWidthPadding;

            block._NodeRect = DecideBlockNodeRectSize();
            Rect DecideBlockNodeRectSize()
            {
                Rect result = block._NodeRect;
                result.width = Mathf.Clamp(nodeWidthA, blockMinWidth, blockMaxWidth);
                result.height = defaultBlockHeight;
                if (AmanitaEditorPreferences.useGridSnap)
                {
                    result = result.SnapWidth(gridObjectSnap);
                }
                return result;
            }


            var graphics = drawCtx.Graphics;
            Rect windowRelativeRect = block._NodeRect;
            var tmpNormBg = nodeStyle.normal.background;

            // Draw untinted highlight
            if (block.IsSelected && !block.IsControlSelected)
            {
                GUI.backgroundColor = Color.white;
                nodeStyle.normal.background = graphics.onTexture;
                GUI.Box(windowRelativeRect, "", nodeStyle);
                nodeStyle.normal.background = tmpNormBg;
            }

            if (block.IsControlSelected && !block.IsSelected)
            {
                GUI.backgroundColor = Color.white;
                nodeStyle.normal.background = graphics.onTexture;
                var c = GUI.backgroundColor;
                c.a = 0.5f;
                GUI.backgroundColor = c;
                GUI.Box(windowRelativeRect, "", nodeStyle);
                nodeStyle.normal.background = tmpNormBg;
            }

            // Draw tinted block; ensure text is readable
            var brightness = graphics.tint.r * 0.3 + graphics.tint.g * 0.59 + graphics.tint.b * 0.11;
            var tmpNormTxtCol = nodeStyle.normal.textColor;
            nodeStyle.normal.textColor = brightness >= 0.5 ? Color.black : Color.white;

            SetBlockOpacity();
            void SetBlockOpacity()
            {
                switch (block.FilterState)
                {
                    case Block.FilteredState.Full:
                        break;
                    case Block.FilteredState.Partial:
                        graphics.tint.a *= 0.65f;
                        break;
                    case Block.FilteredState.None:
                        graphics.tint.a *= 0.2f;
                        break;
                    default:
                        break;
                }
            }

            nodeStyle.normal.background = graphics.offTexture;
            GUI.backgroundColor = graphics.tint;
            GUI.Box(windowRelativeRect, block.BlockName, nodeStyle);

            GUI.backgroundColor = Color.white;

            DrawDesc();
            void DrawDesc()
            {
                if (block.Description.Length > 0)
                {
                    var content = new GUIContent(block.Description);
                    windowRelativeRect.y += windowRelativeRect.height;
                    windowRelativeRect.height = descriptionStyle.CalcHeight(content, windowRelativeRect.width);
                    GUI.Label(windowRelativeRect, content, descriptionStyle);
                }
            }

            GUI.backgroundColor = Color.white;

            nodeStyle.normal.textColor = tmpNormTxtCol;
            nodeStyle.normal.background = tmpNormBg;

            // Draw Event Handler labels
            DrawEventHandlerLabels();
            void DrawEventHandlerLabels()
            {
                if (block._EventHandler != null)
                {
                    string handlerLabel = "";
                    var eventType = block._EventHandler.GetType();
                    EventHandlerInfoAttribute info = EventHandlerEditor.GetEventHandlerInfo(eventType);
                    if (info != null)
                    {
                        ObsoleteAttribute obsAttr = eventType.GetCustomAttribute<System.ObsoleteAttribute>();
                        if (obsAttr != null)
                        {
                            handlerLabel = "<" + AmanitaConstants.UIPrefixForDeprecated_RichText + info.EventHandlerName + "> ";
                        }
                        else
                        {
                            handlerLabel = "<" + info.EventHandlerName + "> ";
                        }
                    }

                    Rect rect = new Rect(block._NodeRect);
                    rect.height = handlerStyle.CalcHeight(new GUIContent(handlerLabel), block._NodeRect.width);
                    rect.x += currentFlowchart.ScrollPos.x;
                    rect.y += currentFlowchart.ScrollPos.y - rect.height;

                    GUI.Label(rect, handlerLabel, handlerStyle);
                }
            }
        }

        protected virtual BlockGraphics GetBlockGraphics(Block block)
        {
            var graphics = new BlockGraphics();

            blockGraphicsUniqueListWorkSpace.Clear();
            blockGraphicsConnectedWorkSpace.Clear();
            Color defaultTint;
            if (block._EventHandler != null)
            {
                graphics.offTexture = AmanitaEditorResources.EventNodeOff;
                graphics.onTexture = AmanitaEditorResources.EventNodeOn;
                defaultTint = AmanitaConstants.DefaultEventBlockTint;
            }
            else
            {
                // Count the number of unique connections (excluding self references)
                block.GetConnectedBlocks(ref blockGraphicsConnectedWorkSpace);
                foreach (var connectedBlock in blockGraphicsConnectedWorkSpace)
                {
                    if (connectedBlock == block ||
                        blockGraphicsUniqueListWorkSpace.Contains(connectedBlock))
                    {
                        continue;
                    }
                    blockGraphicsUniqueListWorkSpace.Add(connectedBlock);
                }

                if (blockGraphicsUniqueListWorkSpace.Count > 1)
                {
                    graphics.offTexture = AmanitaEditorResources.ChoiceNodeOff;
                    graphics.onTexture = AmanitaEditorResources.ChoiceNodeOn;
                    defaultTint = AmanitaConstants.DefaultChoiceBlockTint;
                }
                else
                {
                    graphics.offTexture = AmanitaEditorResources.ProcessNodeOff;
                    graphics.onTexture = AmanitaEditorResources.ProcessNodeOn;
                    defaultTint = AmanitaConstants.DefaultProcessBlockTint;
                }
            }

            graphics.tint = (block.UseCustomTint ? block.Tint : defaultTint) * AmanitaEditorPreferences.flowchartBlockTint;

            return graphics;
        }

        static protected IList<Block> blockGraphicsUniqueListWorkSpace = new List<Block>();
        static protected List<Block> blockGraphicsConnectedWorkSpace = new List<Block>();

    }

    public class DrawBlockContext
    {
        public virtual FlowchartContext FlowchartCtx { get; set; }
        public virtual float BlockMinWidth { get; set; } = 60;
        public virtual float BlockMaxWidth { get; set; } = 240;
        public virtual float DefaultBlockHeight { get; set; } = 40;
        public virtual bool UseGridSnap { get { return AmanitaEditorPreferences.useGridSnap; } }
        public virtual float GridObjectSnap { get; set; } = 20;
        public virtual GUIStyle NodeStyle { get; set; }
        public virtual GUIStyle DescriptionStyle { get; set; }
        public virtual GUIStyle HandlerStyle { get; set; }
        public virtual GUIStyle BlockSearchPopupNormalStyle { get; set; }
        public virtual GUIStyle BlockSearchPopupSelectedStyle { get; set; }
        public virtual BlockGraphics Graphics { get; set; }
        public virtual IList<Block> AllBlocks { get { return FlowchartCtx.AllBlocks; } }
        public virtual Rect ViewRect { get; set; }
    }

    public interface IBlockGraphicsGenerator
    {
        BlockGraphics GenerateFor(Block block);
    }

    public class BlockGraphicsGenerator : IBlockGraphicsGenerator
    {
        public virtual BlockGraphics GenerateFor(Block block)
        {
            var graphics = new BlockGraphics();

            blockGraphicsUniqueListWorkSpace.Clear();
            blockGraphicsConnectedWorkSpace.Clear();
            Color defaultTint;
            if (block._EventHandler != null)
            {
                graphics.offTexture = AmanitaEditorResources.EventNodeOff;
                graphics.onTexture = AmanitaEditorResources.EventNodeOn;
                defaultTint = AmanitaConstants.DefaultEventBlockTint;
            }
            else
            {
                // Count the number of unique connections (excluding self references)
                block.GetConnectedBlocks(ref blockGraphicsConnectedWorkSpace);
                foreach (var connectedBlock in blockGraphicsConnectedWorkSpace)
                {
                    if (connectedBlock == block ||
                        blockGraphicsUniqueListWorkSpace.Contains(connectedBlock))
                    {
                        continue;
                    }
                    blockGraphicsUniqueListWorkSpace.Add(connectedBlock);
                }

                if (blockGraphicsUniqueListWorkSpace.Count > 1)
                {
                    graphics.offTexture = AmanitaEditorResources.ChoiceNodeOff;
                    graphics.onTexture = AmanitaEditorResources.ChoiceNodeOn;
                    defaultTint = AmanitaConstants.DefaultChoiceBlockTint;
                }
                else
                {
                    graphics.offTexture = AmanitaEditorResources.ProcessNodeOff;
                    graphics.onTexture = AmanitaEditorResources.ProcessNodeOn;
                    defaultTint = AmanitaConstants.DefaultProcessBlockTint;
                }
            }

            graphics.tint = (block.UseCustomTint ? block.Tint : defaultTint) * AmanitaEditorPreferences.flowchartBlockTint;

            return graphics;
        }

        static protected IList<Block> blockGraphicsUniqueListWorkSpace = new List<Block>();
        static protected List<Block> blockGraphicsConnectedWorkSpace = new List<Block>();
    }

    public interface INodeStyleProvider
    {
        void ProvideStylesTo(DrawBlockContext ctx);
    }

    public class NodeStyleProvider : INodeStyleProvider
    {
        // cache styles here, rather than duping them for every block we may ever draw,
        // does mean any modifications made to the style when drawing must be undone as you go
        // ^The comment that was above InitStyles in an older ver of FlowchartWindow.cs
        public virtual void ProvideStylesTo(DrawBlockContext ctx)
        {
            PrepStyles();
            void PrepStyles()
            {
                // To reduce GC cruft, we want to cache the styles we provide
                if (nodeStyle == null)
                {
                    nodeStyle = new GUIStyle();
                }

                // All block nodes use the same GUIStyle, but with a different background
                nodeStyle.border = new RectOffset(HorizontalPad, HorizontalPad,
                    VerticalPad, VerticalPad);
                nodeStyle.padding = nodeStyle.border;
                nodeStyle.contentOffset = Vector2.zero;
                nodeStyle.alignment = TextAnchor.MiddleCenter;
                nodeStyle.wordWrap = true;

                if (EditorStyles.helpBox != null && descriptionStyle == null)
                {
                    descriptionStyle = new GUIStyle(EditorStyles.helpBox);
                }
                descriptionStyle.wordWrap = true;

                if (EditorStyles.whiteLabel != null && handlerStyle == null)
                {
                    handlerStyle = new GUIStyle(EditorStyles.label);
                }
                handlerStyle.wordWrap = true;
                handlerStyle.margin.top = 0;
                handlerStyle.margin.bottom = 0;
                handlerStyle.alignment = TextAnchor.MiddleCenter;

                if (blockSearchPopupNormalStyle == null || blockSearchPopupSelectedStyle == null)
                {
                    blockSearchPopupNormalStyle = new GUIStyle(GUI.skin.FindStyle("MenuItem"));
                }
                blockSearchPopupNormalStyle.padding = new RectOffset(8, 0, 0, 0);
                blockSearchPopupNormalStyle.imagePosition = ImagePosition.ImageLeft;
                blockSearchPopupSelectedStyle = new GUIStyle(blockSearchPopupNormalStyle);
                blockSearchPopupSelectedStyle.normal = blockSearchPopupSelectedStyle.hover;
                blockSearchPopupNormalStyle.hover = blockSearchPopupNormalStyle.normal;
            }

            DoTheProviding();
            void DoTheProviding()
            {
                ctx.NodeStyle = nodeStyle;
                ctx.DescriptionStyle = descriptionStyle;
                ctx.HandlerStyle = handlerStyle;
                ctx.BlockSearchPopupNormalStyle = blockSearchPopupNormalStyle;
                ctx.BlockSearchPopupSelectedStyle = blockSearchPopupSelectedStyle;
            }
        }

        protected GUIStyle nodeStyle, descriptionStyle,
            handlerStyle, blockSearchPopupNormalStyle,
            blockSearchPopupSelectedStyle;

        public virtual int HorizontalPad { get; set; } = 20;
        public virtual int VerticalPad { get; set; } = 5;
    }

    public struct BlockGraphics
    {
        internal Color tint;
        internal Texture2D onTexture;
        internal Texture2D offTexture;
    }
}