using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;
using System;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles drawing all the Blocks in the currently-selected Flowchart
    /// </summary>
    public class BlockRenderer : IDisposable
    {
        public virtual void Dispose()
        {
            _drawer = null;
            _graphicsGenerator = null;
        }

        public BlockRenderer(IBlockDrawer drawer, IBlockGraphicsGenerator graphicsGenerator)
        {
            _drawer = drawer;
            _graphicsGenerator = graphicsGenerator;
        }

        protected IBlockDrawer _drawer;
        protected IBlockGraphicsGenerator _graphicsGenerator;

        public virtual void Render(DrawBlockContext drawCtx)
        {
            var fc = drawCtx.FlowchartCtx.Flowchart;
            var viewRect = drawCtx.ViewRect;

            foreach (var block in drawCtx.FlowchartCtx.AllBlocks)
            {
                // size in model-space
                var content = new GUIContent(block.BlockName);
                var textSize = drawCtx.NodeStyle.CalcSize(content);
                const float pad = 10f;

                Rect modelRect = block._NodeRect;
                modelRect.width = Mathf.Clamp(textSize.x + pad, drawCtx.BlockMinWidth, drawCtx.BlockMaxWidth);
                modelRect.height = drawCtx.DefaultBlockHeight;
                if (drawCtx.UseGridSnap)
                    modelRect = modelRect.SnapPosition(drawCtx.GridObjectSnap);

                // scroll (no manual zoom here—EditorZoomArea handles that)
                Rect windowRect = modelRect;
                windowRect.position += fc.ScrollPos;

                // clip
                if (!viewRect.Overlaps(windowRect))
                    continue;

                // stash it and draw
                drawCtx.CurrentBlockWindowRect = windowRect;
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
        public void Draw(Block block, DrawBlockContext ctx)
        {
            var rect = ctx.CurrentBlockWindowRect;    // THIS is screen-space in zoomed coords

            var graphics = ctx.Graphics;
            var style = ctx.NodeStyle;
            var savedBg = style.normal.background;
            var savedTxt = style.normal.textColor;
            
            GUIStyle nodeStyle = ctx.NodeStyle;

            // highlight
            if (block.IsSelected && !block.IsControlSelected)
            {
                //Debug.Log($"Block {block.BlockName} is selected");
                GUI.backgroundColor = Color.white;
                style.normal.background = graphics.onTexture;
                GUI.Box(rect, "", style);
                style.normal.background = savedBg;
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
            GUI.Box(rect, block.BlockName, nodeStyle);

            GUI.backgroundColor = Color.white;

            // description
            if (!string.IsNullOrEmpty(block.Description))
            {
                var descRect = rect;
                descRect.y += rect.height;
                descRect.height = ctx.DescriptionStyle.CalcHeight(
                    new GUIContent(block.Description), rect.width);
                GUI.Label(descRect, block.Description, ctx.DescriptionStyle);
            }

            // restore state
            style.normal.textColor = savedTxt;
            GUI.backgroundColor = Color.white;
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

    public class DrawBlockContext : IDisposable
    {
        public virtual void Dispose()
        {
            BlockMinWidth = 60;
            BlockMinWidth = 240;
            DefaultBlockHeight = 40;
            NodeStyle = DescriptionStyle = HandlerStyle = BlockSearchPopupNormalStyle =
                BlockSearchPopupSelectedStyle = null;
            Graphics = default;
            ViewRect = CurrentBlockWindowRect = default;
        }

        public virtual FlowchartContext FlowchartCtx { get; set; }
        public virtual float BlockMinWidth { get; set; } = 60;
        public virtual float BlockMaxWidth { get; set; } = 240;
        public virtual float DefaultBlockHeight { get; set; } = 40;
        public virtual bool UseGridSnap { get { return AmanitaEditorPreferences.useGridSnap; } }
        public virtual float GridObjectSnap { get { return FlowchartCtx.GridObjectSnap; } }
        public virtual GUIStyle NodeStyle { get; set; }
        public virtual GUIStyle DescriptionStyle { get; set; }
        public virtual GUIStyle HandlerStyle { get; set; }
        public virtual GUIStyle BlockSearchPopupNormalStyle { get; set; }
        public virtual GUIStyle BlockSearchPopupSelectedStyle { get; set; }
        public virtual BlockGraphics Graphics { get; set; }
        public virtual IList<Block> AllBlocks { get { return FlowchartCtx.AllBlocks; } }
        public virtual Rect ViewRect { get; set; }
        public Rect CurrentBlockWindowRect { get; set; }

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