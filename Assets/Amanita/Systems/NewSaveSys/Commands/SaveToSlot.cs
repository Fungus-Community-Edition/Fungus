using UnityEngine;
using Amanita.VScripting;
using System.Threading.Tasks;
using System.Collections;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("Save Sys",
        "Save to Slot",
        "As it says on the tin.")]
    public class SaveToSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(0);
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(false);

        public override void OnEnter()
        {
            bool validSlotIndex = slotIndex != null && slotIndex.Value >= SaveSystem.minSlotNumber;
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
                Task saveTask = SaveSystem.S.SaveTo(slotIndex.Value);
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
            string result = $"Save to Slot {slotIndex.Value}";
            return result;
        }
    }
}