using UnityEngine;
using Amanita.VScripting;
using System.Threading.Tasks;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("SaveSys",
        "Save to Slot",
        "As it says on the tin.")]
    public class SaveToSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(0);
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(false);

        public override async void Execute()
        {
            Task saveTask = SaveSystem.S.SaveTo(slotIndex.Value);
            if (waitUntilFinished.Value)
            {
                await saveTask;
            }
            Continue();
        }

        public override string GetSummary()
        {
            string result = $"Save to Slot {slotIndex.Value}";
            return result;
        }
    }
}