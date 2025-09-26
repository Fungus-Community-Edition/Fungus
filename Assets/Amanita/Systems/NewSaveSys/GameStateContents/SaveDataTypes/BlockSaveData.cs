using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// The state for a single Block.
    /// </summary>
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

        public override SaveDataUnit Serialized()
        {
            var dataAsJson = JsonUtility.ToJson(this, true);
            SaveDataUnit data = new(TypeName, dataAsJson);
            return data;
        }

        public static BlockSaveData DeserializeFrom(SaveDataUnit item)
        {
            ValidateSerializedData(item, nameof(BlockSaveData));
            BlockSaveData data = JsonUtility.FromJson<BlockSaveData>(item.Content);
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
