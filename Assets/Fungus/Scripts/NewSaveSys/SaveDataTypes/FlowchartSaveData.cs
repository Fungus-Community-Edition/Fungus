using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class FlowchartSaveData : SaveData
    {
        // When finding which flowchart this should be applied to, we search
        // by ID first. If not found, then we search by name.
        [SerializeField] protected string uniqueID = string.Empty;
        [SerializeField] protected string flowchartName = string.Empty;
        [SerializeField] protected List<VariableSaveData> savedVars = new();
        [SerializeField] protected List<BlockSaveData> savedBlocks = new();

        public virtual string UniqueId
        {
            get => uniqueID;
            set => uniqueID = value;
        }

        public virtual string FlowchartName
        {
            get => flowchartName;
            set => flowchartName = value;
        }

        public virtual IList<VariableSaveData> SavedVars
        {
            get => savedVars;
            set
            {
                savedVars.Clear();
                savedVars.AddRange(value);
            }
        }

        public virtual IList<BlockSaveData> SavedBlocks
        {
            get => savedBlocks;
            set
            {
                savedBlocks.Clear();
                savedBlocks.AddRange(value);
            }
        }

        public FlowchartSaveData()
        {
            // Default constructor for serialization
        }

        public override SaveDataUnit Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            string typeName = GetType().Name;
            SaveDataUnit newItem = new(typeName, json);
            return newItem;
        }

    }

}