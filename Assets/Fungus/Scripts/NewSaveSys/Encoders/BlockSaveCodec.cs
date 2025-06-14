using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "BlockSaveEncoder", menuName = "Amanita/SaveSys/Encoders/BlockSaveEncoder")]
    public class BlockSaveCodec : SaveCodec<Block, BlockSaveData>,
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

        public override SaveData DecodeFrom(SaveDataUnit unit)
        {
            BlockSaveData result = JsonUtility.FromJson<BlockSaveData>(unit.Content);
            return result;
        }

        public virtual IList<BlockSaveData> EncodeToMultiSave(Flowchart withTheBlocks)
        {
            IList<Block> blocksToConsider = (from elem in withTheBlocks.GetExecutingBlocks()
                                             where elem.SaveExecutionState == true
                                             select elem).ToList();
            IList<BlockSaveData> blockSaves = blocksToConsider
                .Select(block => EncodeToSave(block))
                .ToList();
            return blockSaves;
        }

        public virtual IList<BlockSaveData> EncodeToMultiSaves(IList<Block> toCreateFrom)
        {
            List<BlockSaveData> blockSaves = toCreateFrom
                .Select(block => EncodeToSave(block))
                .ToList();
            return blockSaves;
        }

        public override BlockSaveData EncodeToSave(Block toCreateFrom)
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

            BlockSaveData blockSave = new()
            {
                ItemId = itemId,
                BlockName = blockName,
                ActiveCommandId = activeCommandId,
                ActiveCommandIndex = activeCommandIndex,
            };

            return blockSave;
        }

        public override SaveDataUnit EncodeToUnit(Block from)
        {
            BlockSaveData blockSaveData = EncodeToSave(from);
            SaveDataUnit result = blockSaveData.Serialized();
            return result;
        }

        public override SaveDataUnit EncodeToUnit()
        {
            return EncodeToUnit(ToMakeFrom);
        }

        public override IList<SaveDataUnit> FindAndEncodeAll()
        {
            throw new System.NotImplementedException();
        }
    }
}