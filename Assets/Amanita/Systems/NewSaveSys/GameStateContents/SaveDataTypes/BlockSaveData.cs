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
        [SerializeField] protected ushort itemId = 0;
        [SerializeField] protected int activeCommandId = -1;
        [SerializeField] protected int activeCommandIndex = -1;
        public virtual ushort ItemId
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

        public static readonly BlockSaveData Null = new()
        {
            ItemId = 0,
            BlockName = "Null",
            ActiveCommandId = -1,
            ActiveCommandIndex = -1
        };
    }
}
