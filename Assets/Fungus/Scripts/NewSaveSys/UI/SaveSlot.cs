using System.Collections.Generic;
using System.Linq;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class SaveSlot
    {
        public int SlotIndex { get; set; }
        public string SlotName { get; set; } = string.Empty; // Optional, for user-friendly display
        public string LastSavedUtc { get; set; } = string.Empty;
        public float SaveVersion { get; set; } = -1;
        public SaveDataUnit[] SaveDataItems { get; set; } = new SaveDataUnit[0];

        public SaveSlot(int slotIndex, string slotName,
            IList<SaveDataUnit> saveDataItems, float saveVersion, string lastSavedUtc)
        {
            SlotIndex = slotIndex;
            SlotName = slotName;
            SaveDataItems = saveDataItems.ToArray();
            SaveVersion = saveVersion;
            LastSavedUtc = lastSavedUtc;
        }
    }
}