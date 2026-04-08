using AtMycelia.Amanita.EditorUtils;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
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
                //graphics.offTexture = AmanitaEditorResources.EventNodeOff;
                //graphics.onTexture = AmanitaEditorResources.EventNodeOn;
                defaultTint = HyphlowConstants.DefaultEventBlockTint;
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
                    //graphics.offTexture = AmanitaEditorResources.ChoiceNodeOff;
                    //graphics.onTexture = AmanitaEditorResources.ChoiceNodeOn;
                    defaultTint = HyphlowConstants.DefaultChoiceBlockTint;
                }
                else
                {
                    //graphics.offTexture = AmanitaEditorResources.ProcessNodeOff;
                    //graphics.onTexture = AmanitaEditorResources.ProcessNodeOn;
                    defaultTint = HyphlowConstants.DefaultProcessBlockTint;
                }
            }

            graphics.tint = block.UseCustomTint ? 
                block.Tint : 
                defaultTint * AmanitaEditorPreferences.flowchartBlockTint;

            return graphics;
        }

        static protected IList<Block> blockGraphicsUniqueListWorkSpace = new List<Block>();
        static protected List<Block> blockGraphicsConnectedWorkSpace = new List<Block>();
    }

    public interface IBlockGraphicsGenerator
    {
        BlockGraphics GenerateFor(Block block);
    }

    public struct BlockGraphics
    {
        internal Color tint;
        internal Texture2D onTexture;
        internal Texture2D offTexture;
    }
}