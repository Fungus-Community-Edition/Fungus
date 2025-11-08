using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("SaveSys",
        "Save to Slot",
        "As it says on the tin.")]
    public class SaveToSlot : Command
    {
        [SerializeField] protected IntegerData slotIndex = new IntegerData(0);

        public override void Execute()
        {
            SaveSystem.S.SaveTo(slotIndex.Value);
            Continue();
        }
    }
}