using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
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

}