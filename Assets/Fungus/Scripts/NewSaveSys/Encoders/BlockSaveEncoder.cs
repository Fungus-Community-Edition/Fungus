using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "BlockSaveEncoder", menuName = "Amanita/SaveSys/Encoders/BlockSaveEncoder")]
    public class BlockSaveEncoder : SaveEncoder<Block, BlockSaveData>,
        IMultiSaveEncoder<Flowchart, BlockSaveData>
    {
        public override bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom.GetType().Name);
        }

        public override bool CanHandle(string typeName)
        {
            return typeName == nameof(Flowchart);
        }
        public virtual IList<BlockSaveData> EncodeMulti(Flowchart withTheBlocks)
        {
            IList<BlockSaveData> blockSaves = withTheBlocks.GetExecutingBlocks()
                .Select(block => Encode(block))
                .ToList();
            return blockSaves;
        }

        public virtual IList<BlockSaveData> Encode(IList<Block> toCreateFrom)
        {
            List<BlockSaveData> blockSaves = toCreateFrom
                .Select(block => Encode(block))
                .ToList();
            return blockSaves;
        }

        public override BlockSaveData Encode(Block toCreateFrom)
        {
            // We assume that the Block was indeed executing at this point.
            int itemId = toCreateFrom.ItemId;
            string blockName = toCreateFrom.BlockName;
            int activeCommandId = -1, activeCommandIndex = -1;

            if (toCreateFrom.ActiveCommand != null)
            {
                activeCommandId = toCreateFrom.ActiveCommand.ItemId;
                activeCommandIndex = toCreateFrom.ActiveCommand.CommandIndex;
            }
            else
            {
                Debug.LogWarning($"Block {blockName} does not have an active command.");
            }

            BlockSaveData blockSave = new()
            {
                ItemId = itemId,
                BlockName = blockName,
                ActiveCommandId = activeCommandId,
                ActiveCommandIndex = activeCommandIndex,
            };

            return blockSave;
        }

    }
}