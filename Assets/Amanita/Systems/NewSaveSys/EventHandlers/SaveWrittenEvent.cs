using AtMycelia.Amanita.VScripting;
using AtMycelia.Amanita.VScripting.EventHandlers;
using AtMycelia.SaveSys;
using UnityEngine;

namespace AtMycelia.Amanita.SaveSys.VScripting
{
    [EventHandlerInfo("SaveSys",
        "Save Written",
        "Triggered right before or after (user's choice) a save file has been successfully written.")]
    public class SaveWrittenEvent : EventHandler
    {
        [SerializeField] private ResponseTiming responseTiming = ResponseTiming.After;

        [Tooltip("The slot number of the save file will be assigned to the IntMuscariable assigned here (if any).")]
        [ContentTypeConstraint(typeof(int))]
        [SerializeField] private VariableReference _slotNumber;

        public enum ResponseTiming
        {
            Before,
            After
        }

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);

            if (on)
            {
                SaveSysSignals.PreSaveWrittenToEmptySlot += OnPreSaveWritten;
                SaveSysSignals.PostSaveWrittenToEmptySlot += OnPostSaveWritten;

                SaveSysSignals.PreSaveOverwritten += OnPreSaveWritten;
                SaveSysSignals.PostSaveOverwritten += OnPostSaveWritten;
            }
            else
            {
                SaveSysSignals.PreSaveOverwritten -= OnPreSaveWritten;
                SaveSysSignals.PostSaveOverwritten -= OnPostSaveWritten;

                SaveSysSignals.PreSaveWrittenToEmptySlot -= OnPreSaveWritten;
                SaveSysSignals.PostSaveWrittenToEmptySlot -= OnPostSaveWritten;
            }
        }

        private void OnPreSaveWritten(SaveWriteRequest request)
        {
            if (responseTiming == ResponseTiming.After)
            {
                return;
            }

            UpdateSlotNumberVal(request.SlotNumber);
            ExecuteBlock();
        }

        private void OnPostSaveWritten(SaveWriteResults results)
        {
            if (responseTiming == ResponseTiming.Before)
            {
                return;
            }

            UpdateSlotNumberVal(results.SlotNumber);
            ExecuteBlock();
        }

        private void UpdateSlotNumberVal(int val)
        {
            if (_slotNumber != null && _slotNumber.Variable != null)
            {
                _slotNumber?.SetValue(val);
            }
        }

        public override string DisplayNameAboveBlock => $"Save Written ({responseTiming})";
    }
}