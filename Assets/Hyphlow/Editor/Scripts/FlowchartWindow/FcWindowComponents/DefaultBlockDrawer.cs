using System;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Default UITK drawer that produces tinted buttons sized to block text.
    /// </summary>
    public sealed class DefaultBlockDrawer : IBlockDrawerUitk
    {
        private readonly IBlockGraphicsGenerator graphicsGenerator;

        public DefaultBlockDrawer()
            : this(new BlockGraphicsGenerator())
        {
        }

        public DefaultBlockDrawer(IBlockGraphicsGenerator graphicsGenerator)
        {
            this.graphicsGenerator = graphicsGenerator ??
                throw new ArgumentNullException(nameof(graphicsGenerator));
        }

        public BlockButton CreateButton(Block block)
        {
            FlowchartWindowConfig config = FlowchartWindow.Config;
            VisualTreeAsset blockTemplate = config != null ? config.BlockUxml : null;
            StyleSheet baseStyleSheet = config != null ? config.BlockStyleSheet : null;
            StyleSheet selectedStyleSheet = config != null ? config.SelectedBlockStyleSheet : null;

            var button = new BlockButton(graphicsGenerator);
            button.Initialize(block, blockTemplate, baseStyleSheet, selectedStyleSheet);
            return button;
        }

        public void UpdateButton(BlockButton button, Block block, float zoom)
        {
            if (button == null || block == null)
            {
                return;
            }

            button.UpdateVisuals(block, zoom);
        }
    }

    public interface IBlockDrawer
    {
        void Draw(Block toDraw, DrawBlockContext drawCtx);
    }
}