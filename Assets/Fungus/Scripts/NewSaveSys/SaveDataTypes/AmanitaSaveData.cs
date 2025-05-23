using UnityEngine;
using System.Collections.Generic;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Holds the state for everything that is saved in Amanita's default save system. This
    /// includes the state of the Flowchart and all its variables, as well as
    /// the state of Myceliaudio.
    /// </summary>
    [System.Serializable]
    public class AmanitaSaveData : SaveData
    {
        public virtual IList<SaveData> AllSaves { get; set; } = new List<SaveData>();
        [SerializeField] protected List<FlowchartData> flowchartSaves = new List<FlowchartData>();
        // ... Myceliaudio state
        [SerializeField] protected MyceliaudioSaveData myceliaudioSave = new MyceliaudioSaveData();

        public override SerializedSaveData Serialized()
        {
            string json = JsonUtility.ToJson(this, true);
            SerializedSaveData result = new SerializedSaveData(TypeName, json);
            return result;
        }
    }
}