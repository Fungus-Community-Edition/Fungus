using AtMycelia.SaveSys.UI;
using AtMycelia.Amanita.VScripting;
using UnityEngine;

namespace AtMycelia.SaveSys.VScripting
{
    [CommandInfo("Save Sys/DebugOnly", 
        "ClickSaveSlotUI", 
        "Triggers the click of a specified save slot ui in the save menu.")]
    public class ClickSaveSlotUI : Command
    {
        [SerializeField] private IntegerData _slotIndex = new IntegerData(1);

        public override void OnEnter()
        {
            if (!Application.isEditor)
            {
                Continue();
                return;
            }

            base.OnEnter();
            bool validSlotIndex = _slotIndex > 0;
            if (!validSlotIndex)
            {
                string errorMessage = $"ClickSaveSlotUI: Invalid slot index {_slotIndex.Value}. Must be greater than 0.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }

            SaveSlotUIManager slotUiManager = FindFirstObjectByType<SaveSlotUIManager>();
            if (slotUiManager == null)
            {
                string errorMessage = "ClickSaveSlotUI: No SaveSlotUIManager found in the scene.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }

            // At this point, we assume that the slot index is valid (1-based index)
            var slotUIs = slotUiManager.SlotUIs;
            if (_slotIndex >= slotUIs.Count)
            {
                string errorMessage = $"ClickSaveSlotUI: Slot index {_slotIndex.Value} is out of range. Total slots: {slotUIs.Count}.";
                Debug.LogError(errorMessage);
                Continue();
                return;
            }

            SaveSlotViewComposer targetSlotUI = slotUIs[_slotIndex - 1]; 
            // ^The slot indexes are 1-based, but the list is 0-based
            targetSlotUI.TriggerClick();
            Continue();
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_slotIndex);
        }

        public override string GetSummary()
        {
            string result = $"At Index {_slotIndex.Value}";
            return result;
        }
    }
}