using UnityEngine;
using AtMycelia.Amanita.VScripting;
using System.Threading.Tasks;
using System.Collections;
using UnityEditor;
using UnityEngine.Serialization;

namespace AtMycelia.SaveSys.VScripting
{
    [CommandInfo("Save Sys",
        "Save to Slot",
        "As it says on the tin. Note that the lowest valid slot index is 1.")]
    public class SaveToSlot : Command
    {
        [FormerlySerializedAs("slotIndex")]
        [SerializeField] protected IntegerData _slotIndex = new IntegerData(1);
        [Tooltip("If true, this will save to the selected slot instead of the specified slot index.")]
        [FormerlySerializedAs("saveToSelected")]
        [SerializeField] protected BooleanData _saveToSelected = new BooleanData(false);
        [FormerlySerializedAs("waitUntilFinished")]
        [SerializeField] protected BooleanData _waitUntilFinished = new BooleanData(true);
        [Tooltip("Before the save process starts, wait this many seconds. This can be useful if you " +
            "want to ensure that some other Command is executing at the time of saving.")]
        [FormerlySerializedAs("delayBeforeSave")]
        [SerializeField] private FloatData _delayBeforeSave = new FloatData(0);

        public override bool ReexecutableOnLoad => false;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_slotIndex);
            _variableDataCache.Add(_saveToSelected);
            _variableDataCache.Add(_waitUntilFinished);
            _variableDataCache.Add(_delayBeforeSave);
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
                SaveSysSignals.SlotSelected += OnSaveSlotSelected;
            }
            else
            {
                SaveSysSignals.SlotSelected -= OnSaveSlotSelected;
            }
        }

        private void OnSaveSlotSelected(int index)
        {
            selectedSlotIndex = index;
        }

        private int selectedSlotIndex = -1;

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            ToggleSubs(false);
        }

        public override void OnEnter()
        {
            StartCoroutine(TrySaveCoroutine());
        }

        private IEnumerator TrySaveCoroutine()
        {
            int slotIndexToGoWith = DecideSlotIndex();
            bool skipDueToNoSlotSelected = false;
            int DecideSlotIndex()
            {
                int result = -1;
                if (_saveToSelected)
                {
                    // Find the selected slot
                    bool nothingSelected = selectedSlotIndex < 0;
                    if (nothingSelected)
                    {
                        string format = "SaveToSlot Command in Block {0} of {1}'s Flowchart: no slot is currently selected.";
                        string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                            this.gameObject.name);
                        Debug.LogError(errorMessage);
                        skipDueToNoSlotSelected = true;
                    }
                    else
                    {
                        result = selectedSlotIndex;
                    }
                }
                else
                {
                    result = _slotIndex.Value;
                }

                return result;
            }

            if (skipDueToNoSlotSelected)
            {
                Continue();
                yield break;
            }

            bool validSlotIndex = ValidateSlotIndex();
            bool ValidateSlotIndex()
            {
                bool valid = slotIndexToGoWith >= SaveSystem.minSlotNumber;
                if (!valid)
                {
                    string format = "SaveToSlot Command in Block {0} of {1}'s Flowchart: slot index must be at least {2}.";
                    string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                        this.gameObject.name, SaveSystem.minSlotNumber);
                    Debug.LogError(errorMessage);
                }
                return valid;
            }

            if (!validSlotIndex)
            {
                Continue();
                yield break;
            }

            if (!_waitUntilFinished)
            {
                Continue(); // For when we want some other Command to be executing at the time of saving.
            }

            if (_delayBeforeSave > 0)
            {
                yield return new WaitForSeconds(_delayBeforeSave);
            }

            Task saveTask = SaveSystem.SaveToSlotAsync(slotIndexToGoWith);
            yield return WaitForTask(saveTask, _waitUntilFinished);
        }

        public override string GetSummary()
        {
            string result;
            if (_saveToSelected)
            {
                result = "Save to Selected Slot";
            }
            else
            {
                result = $"Save to Slot {_slotIndex}";
            }
            
            if (_delayBeforeSave > 0)
            {
                result += $" after {_delayBeforeSave} seconds";
            }
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!gameObject.scene.IsValid())
            {
                return;
            }

            EditorApplication.delayCall += ValidateSlotIndex;
        }

        private void ValidateSlotIndex()
        {
            if (this == null || _slotIndex == null || ParentBlock == null)
            {
                return;
            }

            if (_slotIndex.RepresentingVar)
            {
                return;
            }

            if (_slotIndex.Value < SaveSystem.minSlotNumber)
            {
                Debug.LogWarning($"SaveToSlot Command on {gameObject.name}'s " +
                    $"{ParentBlock.BlockName} Block, index {CommandIndex}: slot index cannot be less " +
                    $"than {SaveSystem.minSlotNumber}. Resetting to {SaveSystem.minSlotNumber}.");
                _slotIndex.Value = SaveSystem.minSlotNumber;
            }
        }
    }
}