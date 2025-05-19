using UnityEngine;
using Fungus;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class BlockSaveData : SaveData
    {
        [SerializeField] protected string blockName = string.Empty;
        [SerializeField] protected int itemId = -1;
        [SerializeField] protected int activeCommandId = -1;
        [SerializeField] protected int activeCommandIndex = -1;
        public virtual int ItemId
        {
            get => itemId;
            set => itemId = value;
        }
        public virtual string BlockName
        {
            get => blockName;
            set => blockName = value;
        }

        public virtual int ActiveCommandId
        {
            get => activeCommandId;
            set => activeCommandId = value;
        }
        public virtual int ActiveCommandIndex
        {
            get => activeCommandIndex;
            set => activeCommandIndex = value;
        }
        
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
