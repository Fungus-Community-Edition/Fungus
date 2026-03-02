using UnityEngine;
using AtMycelia.Amanita.VScripting;

namespace AtMycelia.SaveSys.VScripting
{
    [CommandInfo("Save Sys",
        "Delete from Slot",
        "Deletes the save data from a specified slot index.")]
    public class DeleteFromSlot : Command
    {
        [SerializeField] private IntegerData _slotIndex = new IntegerData(1);

        public override void OnEnter()
        {
            base.OnEnter();
            bool validSlotIndex = _slotIndex > 0;
            if (!validSlotIndex)
            {
                string errorMessage = $"DeleteFromSlot: Invalid slot index {_slotIndex.Value}. Must be greater than 0.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }
            SaveSystem saveSystem = SaveSystem.S;
            if (saveSystem == null)
            {
                string errorMessage = "DeleteFromSlot: No SaveSystem instance found.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }
            saveSystem.DeleteSave(_slotIndex);
            bool deletionSuccess = !saveSystem.DoesSaveExist(_slotIndex);
            if (!deletionSuccess)
            {
                string errorMessage = $"DeleteFromSlot: Failed to delete save data at slot {_slotIndex.Value}.";
                Debug.LogError(errorMessage);
            }
            Continue();
        }
        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_slotIndex);
        }

        public override string GetSummary()
        {
            string result = "From slot " + _slotIndex.Value;
            return result;
        }
    }
}