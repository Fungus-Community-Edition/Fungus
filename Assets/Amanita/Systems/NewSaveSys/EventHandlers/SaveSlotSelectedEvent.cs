using Amanita.VScripting.EventHandlers;
using UnityEngine;
using VSEvent = Amanita.VScripting.EventHandlers.EventHandler;
using Amanita.VScripting;

namespace Amanita.SaveSys.VScripting
{
    [EventHandlerInfo("SaveSys",
        "Save Slot Selected",
        "Triggered when a save slot is selected.")]
    public class SaveSlotSelectedEvent : VSEvent
    {
        [Tooltip("The index of the selected save slot.")]
        [VariableProperty(typeof(IntegerVariable), typeof(IntMuscariable))]
        [SerializeReference] protected IVariable<int> saveSlotIndex;

        protected virtual void OnEnable()
        {
            ToggleSubs(true);
        }
        
        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                SaveSysSignals.SaveSlotSelected += HandleSaveSlotSelected;
            }
            else
            {
                SaveSysSignals.SaveSlotSelected -= HandleSaveSlotSelected;
            }
        }

        private void HandleSaveSlotSelected(int index)
        {
            if (saveSlotIndex != null)
            {
                saveSlotIndex.Value = index;
            }

            ExecuteBlock();
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
        }
    }
}