using UnityEngine;
using Amanita.VScripting;
using System.Threading.Tasks;
using System.Collections;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("SaveSys",
        "Save to Slot",
        "As it says on the tin.")]
    public class SaveToSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(0);
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(false);

        public override void Execute()
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

        protected virtual IEnumerator WaitForTask(Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            yield return null; // Just one more frame to ensure any follow-up actions are ready.
            Continue();
        }
        public override string GetSummary()
        {
            string result = $"Save to Slot {slotIndex.Value}";
            return result;
        }
    }
}