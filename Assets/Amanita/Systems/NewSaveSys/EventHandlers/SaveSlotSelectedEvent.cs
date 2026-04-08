using UnityEngine;
using VSEvent = AtMycelia.Hyphlow.EventHandler;

using AtMycelia.SaveSys;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    [EventHandlerInfo("SaveSys",
        "Save Slot Selected",
        "Triggered when a save slot is selected.")]
    public class SaveSlotSelectedEvent : VSEvent
    {
        [Tooltip("The index of the selected save slot.")]
        [ContentTypeConstraint(typeof(int))]
        [SerializeField] protected VariableReference saveSlotIndex = new VariableReference();

        [FormerlySerializedAs("saveSlotIndex")]
        [Tooltip("The index of the selected save slot.")]
        protected IVariable<int> _oldSaveSlotIndex;
        protected override bool ToggleSubsOnlyInRuntime => true;

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            if (on)
            {
                SaveSysSignals.SlotSelected += OnSaveSlotSelected;
            }
            else
            {
                SaveSysSignals.SlotSelected -= OnSaveSlotSelected;
            }
        }

        protected override void OnEnable()
        {
            if (_oldSaveSlotIndex != null)
            {
                saveSlotIndex ??= new VariableReference
                {
                    Variable = _oldSaveSlotIndex
                };
                saveSlotIndex.Variable = _oldSaveSlotIndex;
                _oldSaveSlotIndex = null;
            }
            base.OnEnable();
        }

        protected virtual void OnSaveSlotSelected(int index)
        {
            if (saveSlotIndex != null && saveSlotIndex.Variable != null)
            {
                saveSlotIndex.SetValue(index);
            }

            ExecuteBlock();
        }

    }
}