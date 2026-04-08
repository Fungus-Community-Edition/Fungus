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
        public override bool ReexecutableOnLoad => false;
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

            if (SaveSystem.SaveManager == null)
            {
                string errorMessage = "DeleteFromSlot: SaveSystem is not initialized.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }

            SaveSystem.DeleteSave(_slotIndex);
            bool deletionSuccess = !SaveSystem.DoesSaveExist(_slotIndex);
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
            _variableDataCache.Add(_slotIndex);
        }

        public override string GetSummary()
        {
            string result = "From slot " + _slotIndex.Value;
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            bool literalSlotIndex = _slotIndex.RepresentingVar == false;
            if (literalSlotIndex && _slotIndex < SaveSystem.minSlotNumber)
            {
                Debug.LogWarning($"DeleteFromSlot Command on {this.gameObject.name}: slot index cannot be less " +
                    $"than {SaveSystem.minSlotNumber}. Resetting to {SaveSystem.minSlotNumber}.");
                _slotIndex.Value = SaveSystem.minSlotNumber;
            }
        }
    }
}