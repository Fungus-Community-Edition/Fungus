using AtMycelia.Amanita.VScripting;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.SaveSys.VScripting
{
    [CommandInfo("Save Sys",
        "Load From Slot",
        "As it says on the tin.")]
    public class LoadFromSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(0);
        [Tooltip("If true, this will save to the selected slot instead of the specified slot index.")]
        [SerializeField] protected BooleanData loadFromSelected = new BooleanData(false);
        [SerializeField] protected BooleanData loadScene = new BooleanData(true);
        [Tooltip("If you want this to be true, best make sure that this Command is on a persistent GameObject.")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(false);
        [SerializeField] private FloatData delayBeforeLoad = new FloatData(0);

        public override bool ReexecutableOnLoad => false;

        private int selectedSlotIndex = -1;
        private bool validateScheduled;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(slotIndex);
            variableDataCache.Add(loadFromSelected);
            variableDataCache.Add(loadScene);
            variableDataCache.Add(waitUntilFinished);
            variableDataCache.Add(delayBeforeLoad);
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

        public override void OnEnter()
        {
            if (delayBeforeLoad > 0)
            {
                Invoke(nameof(TryLoad), delayBeforeLoad);
            }
            else
            {
                TryLoad();
            }
        }

        protected virtual void TryLoad()
        {
            int slotIndexToGoWith;
            if (loadFromSelected)
            {
                // Find the selected slot
                // If none is selected, log an error and exit
                if (selectedSlotIndex < 0)
                {
                    string format = "LoadFromSlot Command in Block {0} of {1}'s Flowchart: no slot is currently selected.";
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
                string format = $"LoadFromSlot Command in Block {{0}} of {{1}}'s Flowchart: slot index must be at least {2}. What was given: {3}";
                string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                    this.gameObject.name, SaveSystem.minSlotNumber, slotIndexToGoWith);
                Debug.LogError(errorMessage);
                Continue();
                return;
            }
            else
            {
                Task loadTask = SaveSystem.LoadMainAsync(slotIndex, loadScene);
                if (waitUntilFinished.Value)
                {
                    StartCoroutine(WaitForTask(loadTask));
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
            if (loadFromSelected.Value)
            {
                result = "Load from Selected Slot";
            }
            else
            {
                result = $"Load from Slot {slotIndex.Value}";
            }

            if (delayBeforeLoad > 0)
            {
                result += $" after {delayBeforeLoad.Value} seconds";
            }
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (validateScheduled)
            {
                return;
            }
            validateScheduled = true;
            EditorApplication.delayCall += ValidateSlotIndex;
        }

        private void ValidateSlotIndex()
        {
            EditorApplication.delayCall -= ValidateSlotIndex;
            validateScheduled = false;

            if (this.ParentBlock == null)
            {
                // The parent block being null implies that the slot indexes are not done being rehydrated
                // by Unity's serialization system, so we should hold off on validating until they are.
                // Otherwise, the warnings we log will be misleading, suggesting that a slot index setting
                // is screwed up when (in reality) it just hasn't been loaded by the engine yet.
                return;
            }

            bool literalSlotIndex = slotIndex.RepresentingVar == false;
            if (literalSlotIndex && slotIndex < SaveSystem.minSlotNumber)
            {
                Debug.LogWarning($"LoadFromSlot Command on {this.gameObject.name}'s {this.ParentBlock?.name}: slot index cannot be less " +
                    $"than {SaveSystem.minSlotNumber}. Resetting to {SaveSystem.minSlotNumber}.");
                slotIndex.Value = SaveSystem.minSlotNumber;
            }
            else
            {
                //Debug.Log($"Slot index of {slotIndex.Value} is valid.");
            }
        }
    }
}