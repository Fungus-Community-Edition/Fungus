using UnityEngine;
using Amanita.VScripting;
using System.Threading.Tasks;
using System;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("Save Sys",
        "Save to Slot",
        "As it says on the tin. Note that the lowest valid slot index is 1.")]
    public class SaveToSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(1);
        [Tooltip("If true, this will save to the selected slot instead of the specified slot index.")]
        [SerializeField] protected BooleanData saveToSelected = new BooleanData(false);
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);
        [SerializeField] private FloatData delayBeforeSave = new FloatData(0);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(slotIndex);
            variableDataCache.Add(saveToSelected);
            variableDataCache.Add(waitUntilFinished);
            variableDataCache.Add(delayBeforeSave);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ToggleSubs(true);
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                SaveSysSignals.SaveSlotSelected += OnSaveSlotSelected;
            }
            else
            {
                SaveSysSignals.SaveSlotSelected -= OnSaveSlotSelected;
            }
        }

        private void OnSaveSlotSelected(int index)
        {
            selectedSlotIndex = index;
        }

        private int selectedSlotIndex = -1;

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(slotIndex);
            variableDataCache.Add(waitUntilFinished);
        }

        public override void OnEnter()
        {
            if (delayBeforeSave > 0)
            {
                Invoke(nameof(TrySave), delayBeforeSave);
            }
            else
            {
                TrySave();
            }
        }

        private void TrySave()
        {
            int slotIndexToGoWith;
            if (saveToSelected)
            {
                // Find the selected slot
                // If none is selected, log an error and exit
                if (selectedSlotIndex < 0)
                {
                    string format = "SaveToSlot Command in Block {0} of {1}'s Flowchart: no slot is currently selected.";
                    string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                        this.gameObject.name);
                    Debug.LogError(errorMessage);
                    Continue();
                    return;
                }
                else
                {
                    slotIndexToGoWith = selectedSlotIndex;
                }
            }
            else
            {
                slotIndexToGoWith = slotIndex.Value;
            }

            bool validSlotIndex = slotIndexToGoWith >= SaveSystem.minSlotNumber;
            if (!validSlotIndex)
            {
                string format = "SaveToSlot Command in Block {0} of {1}'s Flowchart: slot index must be at least {2}.";
                string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                    this.gameObject.name, SaveSystem.minSlotNumber);
                Debug.LogError(errorMessage);
                Continue();
                return;
            }
            else
            {
                Task saveTask = SaveSystem.S.SaveToSlotAsync(slotIndexToGoWith);
                if (waitUntilFinished.Value)
                {
                    StartCoroutine(WaitForTask(saveTask));
                }
                else
                {
                    Continue();
                }
            }
        }

        public override string GetSummary()
        {
            // Let's not concern ourselves with whether we're using the selected slot or not.
            // That can dynamically change during runtime, so let's just go with the specified
            // index set here in the editor.
            string result;
            if (saveToSelected.Value)
            {
                result = "Save to Selected Slot";
            }
            else
            {
                result = $"Save to Slot {slotIndex.Value}";
            }
            
            if (delayBeforeSave > 0)
            {
                result += $" after {delayBeforeSave.Value} seconds";
            }
            return result;
        }
    }
}