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

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.IsPlaying(this)) // We only want to subscribe at runtime
            {
                ToggleSubs(true);
            }
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
            // Need to make sure to rehydrate the variable reference, since what we have at this point
            // might be a copy instead of the actual variable reference in the flowchart.
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

        protected override bool RehydrateVarInputs => true;
    }
}