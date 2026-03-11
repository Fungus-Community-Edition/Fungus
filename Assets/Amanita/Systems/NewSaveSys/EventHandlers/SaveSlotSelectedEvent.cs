using AtMycelia.Amanita.VScripting.EventHandlers;
using UnityEngine;
using VSEvent = AtMycelia.Amanita.VScripting.EventHandlers.EventHandler;

using AtMycelia.SaveSys;

namespace AtMycelia.Amanita.VScripting
{
    [EventHandlerInfo("SaveSys",
        "Save Slot Selected",
        "Triggered when a save slot is selected.")]
    public class SaveSlotSelectedEvent : VSEvent
    {
        [Tooltip("The index of the selected save slot.")]
        [VariableProperty(typeof(IntegerVariable), typeof(IntMuscariable))]
        [SerializeReference] protected IVariable<int> saveSlotIndex;

        protected override bool RehydrateVarInputs => true;
        protected override bool ToggleSubsOnlyInRuntime => true;
        
        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            if (on)
            {
                SaveSysSignals.SaveSlotSelected += OnSaveSlotSelected;
            }
            else
            {
                SaveSysSignals.SaveSlotSelected -= OnSaveSlotSelected;
            }
        }

        protected virtual void OnSaveSlotSelected(int index)
        {
            if (saveSlotIndex != null)
            {
                saveSlotIndex.Value = index;
            }

            ExecuteBlock();
        }

    }
}