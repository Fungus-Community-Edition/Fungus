using UnityEngine;
using AtMycelia.SaveSys;

namespace AtMycelia.Amanita.SaveSys
{
    /// <summary>
    /// The state for a single Block.
    /// </summary>
    [System.Serializable]
    public class BlockSaveData : SaveData
    {
        [SerializeField] protected string blockName = string.Empty;
        [SerializeField] protected byte itemId = 0;
        [SerializeField] protected byte activeCommandId = 0;
        [SerializeField] protected byte activeCommandIndex = 0;
        public virtual byte ItemId
        {
            get => itemId;
            set => itemId = value;
        }
        public virtual string BlockName
        {
            get => blockName;
            set => blockName = value;
        }

        public virtual byte ActiveCommandId
        {
            get => activeCommandId;
            set => activeCommandId = value;
        }
        public virtual byte ActiveCommandIndex
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
            ActiveCommandId = 0,
            ActiveCommandIndex = 0
        };
    }
}
