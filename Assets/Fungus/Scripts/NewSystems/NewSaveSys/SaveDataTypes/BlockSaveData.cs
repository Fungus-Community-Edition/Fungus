using UnityEngine;
using Fungus;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class BlockSaveData : SaveData
    {
        public virtual int ItemId { get; set; } = -1;
        public virtual string BlockName { get; set; } = string.Empty;

        public virtual int ActiveCommandId { get; set; } = -1;
        public virtual int ActiveCommandIndex { get; set; } = -1;
        
        public BlockSaveData()
        {

        }

        public BlockSaveData(Block toCreateFrom)
        {
            // We assume that the Block was indeed executing at this point.
            ItemId = toCreateFrom.ItemId;
            BlockName = toCreateFrom.BlockName;
            
            if (toCreateFrom.ActiveCommand != null)
            {
                ActiveCommandId = toCreateFrom.ActiveCommand.ItemId;
                ActiveCommandIndex = toCreateFrom.ActiveCommand.CommandIndex;
            }
        }

        public override SerializedSaveData Serialized()
        {
            var dataAsJson = JsonUtility.ToJson(this, true);
            SerializedSaveData data = new(TypeName, dataAsJson);
            return data;
        }

        public new static BlockSaveData DeserializeFrom(SerializedSaveData item)
        {
            ValidateSerializedData(item, nameof(BlockSaveData));
            BlockSaveData data = JsonUtility.FromJson<BlockSaveData>(item.Data);
            return data;
        }

        public static readonly BlockSaveData Null = new()
        {
            ItemId = -1,
            BlockName = "Null",
            ActiveCommandId = -1,
            ActiveCommandIndex = -1
        };
    }
}
