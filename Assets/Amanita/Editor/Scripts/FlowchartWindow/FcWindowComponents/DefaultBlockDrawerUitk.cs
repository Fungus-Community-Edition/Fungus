using System;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.VisualElement;
using UitkButton = UnityEngine.UIElements.Button;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Default UITK drawer that produces tinted buttons sized to block text.
    /// </summary>
    public sealed class DefaultBlockDrawerUitk : IBlockDrawerUitk
    {
        private readonly IBlockGraphicsGenerator graphicsGenerator;
        private const float MinWidth = 60f;
        private const float MaxWidth = 280f;
        private const float PaddingX = 18f;
        private const float PaddingY = 10f;
        private const float DefaultHeight = 40f;
        private const float BaseFontSize = 16f;
        public static readonly string BaseClass = "flowchartBlock";
        public static readonly string SelectedClass = "flowchartBlockSelected";

        private static readonly Color GradientTop = new Color(1f, 1f, 1f, 0.18f);
        private static readonly Color GradientBottom = new Color(0f, 0f, 0f, 0.25f);

        public DefaultBlockDrawerUitk()
            : this(new BlockGraphicsGenerator())
        {
        }

        public DefaultBlockDrawerUitk(IBlockGraphicsGenerator graphicsGenerator)
        {
            this.graphicsGenerator = graphicsGenerator ?? throw new ArgumentNullException(nameof(graphicsGenerator));
        }


        public UitkButton CreateButton(Block block)
        {
            var button = new UitkButton
            {
                focusable = false,
                text = SafeBlockName(block)
            };

            ApplyStyles();
            void ApplyStyles()
            {
                var config = FlowchartWindowUitk.Config;
                var baseStyleSheet = config.BlockStyleSheet;
                // Apply that first so we can override specific properties below.
                if (baseStyleSheet != null)
                {
                    button.styleSheets.Add(baseStyleSheet);
                }

                var selectedStyleSheet = config.SelectedBlockStyleSheet;
                if (selectedStyleSheet != null)
                {
                    button.styleSheets.Add(selectedStyleSheet);
                }

                button.style.unityFontStyleAndWeight = FontStyle.Normal;

                button.AddToClassList(BaseClass);
                button.AddToClassList(SelectedClass);
                button.EnableInClassList(SelectedClass, false);

                UitkGradientDrawer.AttachVerticalGradient(button, GradientTop, GradientBottom);
            }

            return button;
        }

        public void UpdateButton(UitkButton button, Block block, float zoom)
        {
            if (button == null || block == null)
            {
                return;
            }

            button.text = SafeBlockName(block);

            UpdateFont(button, zoom);
            void UpdateFont(UitkButton button, float zoom)
            {
                button.style.fontSize = Mathf.RoundToInt(BaseFontSize);
                button.transform.scale = new Vector3(zoom, zoom, 1f);
                // ^Scaling only here (and going unscaled in the other calculations) allows us to 
                // make sure that the zoom-based scaling happens properly, making things look right.
            }

            UpdateSize();
            void UpdateSize()
            {
                Vector2 unrestrictedSize = button.MeasureTextSize(
                    button.text,
                    float.NaN,
                    MeasureMode.Undefined,
                    float.NaN,
                    MeasureMode.Undefined);
                // ^So that single-line blocks don't get weird word-wrapping, at least so long
                // as they don't exceed the max width.

                float totalPaddingX = PaddingX * 2f;
                float unclampedWidth = Mathf.Clamp(unrestrictedSize.x + totalPaddingX, MinWidth, MaxWidth);
                float textWidthConstraint = Mathf.Max(unclampedWidth - totalPaddingX, minTextWidth);

                Vector2 wrappedSize = button.MeasureTextSize(
                    button.text,
                    textWidthConstraint,
                    MeasureMode.AtMost,
                    float.NaN,
                    MeasureMode.Undefined);

                float width = unclampedWidth;
                float height = Mathf.Max(DefaultHeight, wrappedSize.y + PaddingY);

                button.style.width = width;
                button.style.height = height;

                Rect newNodeRect = block._NodeRect;
                newNodeRect.width = width;
                newNodeRect.height = height;
                block._NodeRect = newNodeRect;
            }

            bool isSelected;
            UpdateColors();
            void UpdateColors()
            {
                BlockGraphics graphics = graphicsGenerator.GenerateFor(block);
                Color tint = graphics.tint;
                button.style.backgroundColor = new StyleColor(tint);
                button.style.color = new StyleColor(ChooseTextColor(tint));

                isSelected = block.IsSelected && !block.IsControlSelected;
                button.EnableInClassList(SelectedClass, isSelected);
            }
        }

        private static readonly float minTextWidth = 1f;

        private static string SafeBlockName(Block block)
        {
            string result = "New Block";
            if (block != null)
            {
                result = block.BlockName;
                if (result.Length > maxBlockNameLength)
                {
                    result = result[..maxBlockNameLength];
                }
            }

            return result;
        }

        private static readonly int maxBlockNameLength = 50;

        private static Color ChooseTextColor(Color background)
        {
            float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
            return luminance >= 0.5f ? Color.black : Color.white;
        }
    }

    public interface IBlockDrawer
    {
        void Draw(Block toDraw, DrawBlockContext drawCtx);
    }
}