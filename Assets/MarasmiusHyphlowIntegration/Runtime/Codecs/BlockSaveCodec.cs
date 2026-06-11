using System.Collections.Generic;
using AtMycelia.Hyphlow;
using AtMycelia.FSExt;
using FullSerializer;
using AtMycelia.SaveSys;

namespace AtMycelia.Amanita.SaveSys
{
    public class BlockSaveCodec : SaveCodec<IBlock, BlockSaveData>,
        IMultiSaveCodec<Flowchart, BlockSaveData>
    {
        public override bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom.GetType().Name);
        }

        public override bool CanHandle(string typeName)
        {
            return typeName == nameof(Flowchart);
        }

        public override BlockSaveData Decode(string rawText)
        {
            fsSerializer serializer = SaveSystem.DefaultSerializer;
            lock (serializer)
            {
                BlockSaveData result = serializer.FromJson<BlockSaveData>(rawText);
                return result;
            }
        }

        public virtual IList<BlockSaveData> EncodeToMultiSaves(Flowchart withTheBlocks)
        {
            IList<IBlock> blocksToConsider = new List<IBlock>();
            var executingBlocks = withTheBlocks.GetExecutingBlocks();
            for (int i = 0; i < executingBlocks.Count; i++)
            {
                IBlock block = executingBlocks[i];
                if (block.IncludeInSaves)
                {
                    blocksToConsider.Add(block);
                }
            }

            IList<BlockSaveData> blockSaves = EncodeToMultiSaves(blocksToConsider);
            return blockSaves;
        }

        public virtual IList<BlockSaveData> EncodeToMultiSaves(IList<IBlock> toCreateFrom)
        {
            List<BlockSaveData> blockSaves = new List<BlockSaveData>();
            for (int i = 0; i < toCreateFrom.Count; i++)
            {
                IBlock block = toCreateFrom[i];
                if (block.IncludeInSaves == false)
                {
                    continue;
                }
                BlockSaveData blockSave = EncodeToSave(block);
                blockSaves.Add(blockSave);
            }
            return blockSaves;
        }

        public override BlockSaveData EncodeToSave(IBlock toCreateFrom)
        {
            // We assume that the Block was indeed executing at this point.
            byte itemId = toCreateFrom.ItemId;
            string blockName = toCreateFrom.BlockName;
            byte activeCommandId = 0, activeCommandIndex = 0;

            if (toCreateFrom.ActiveCommand != null && toCreateFrom.ActiveCommand.ReexecutableOnLoad)
            {
                activeCommandId = toCreateFrom.ActiveCommand.ItemId;
                activeCommandIndex = toCreateFrom.ActiveCommand.CommandIndex;
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