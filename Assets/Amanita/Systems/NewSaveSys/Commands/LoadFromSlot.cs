using Amanita.VScripting;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("Save Sys",
        "Load From Slot",
        "As it says on the tin.")]
    public class LoadFromSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(0);
        [SerializeField] protected BooleanData loadScene = new BooleanData(true);
        [Tooltip("If you want this to be true, best make sure that this Command is on a persistent GameObject.")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(false);

        public override void OnEnter()
        {
            bool validSlotIndex = slotIndex != null && slotIndex.Value >= SaveSystem.minSlotNumber;
            if (!validSlotIndex)
            {
                string format = "LoadFromSlot Command in Block {0} of {1}'s Flowchart: slot index must be at least {2}.";
                string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                    this.gameObject.name, SaveSystem.minSlotNumber);
                Debug.LogError(errorMessage);
                Continue();
                return;
            }
            else
            {
                Task loadTask = SaveSystem.S.LoadMain(slotIndex.Value, loadScene.Value);
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
            string result = $"Load from Slot {slotIndex.Value}";
            return result;
        }
    }
}