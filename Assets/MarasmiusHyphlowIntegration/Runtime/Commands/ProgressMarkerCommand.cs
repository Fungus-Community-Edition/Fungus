using AtMycelia.Hyphlow;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace AtMycelia.SaveSys.VScripting
{
    [CommandInfo("Save Sys", 
        "Progress Marker", 
        "Marks a point in the game's progress for save/load purposes.")]
    public class ProgressMarkerCommand : Command
    {
        [FormerlySerializedAs("action")]
        [SerializeField] protected PMCAction _action = PMCAction.Register;
        [FormerlySerializedAs("markerID")]
        [SerializeField] protected StringData _markerID = new StringData("DefaultMarker");
        [Tooltip("Determines the order of this marker relative to others. " +
            "Lower numbers indicate earlier execution in SaveDataLoaded events.")]
        [FormerlySerializedAs("markerOrder")]
        [SerializeField] protected IntegerData _markerOrder = new IntegerData(0);
        public enum PMCAction
        {
            Null,
            Register,
            Unregister,
            SetOrder
        }

        protected virtual void Awake()
        {
            actionHandlers[PMCAction.Register] = HandleRegistration;
            actionHandlers[PMCAction.Unregister] = HandleDeregistration;
            actionHandlers[PMCAction.SetOrder] = HandleSettingOrder;
            actionHandlers[PMCAction.Null] = WarnAboutNullInput;
        }

        protected IDictionary<PMCAction, System.Action> actionHandlers = new Dictionary<PMCAction, System.Action>();

        public override void Execute()
        {
            var handler = actionHandlers[_action];
            handler?.Invoke();
            Continue();
        }

        protected virtual void HandleRegistration()
        {
            string id = _markerID.Value;
            int order = _markerOrder.Value;
            SaveSystem.RegisterProgressMarker(id, order);
        }

        protected virtual void HandleDeregistration()
        {
            string id = _markerID.Value;
            SaveSystem.UnregisterProgressMarker(id);
        }

        protected virtual void HandleSettingOrder()
        {
            string id = _markerID.Value;
            int order = _markerOrder.Value;
            SaveSystem.SetProgressMarkerOrder(id, order);
        }

        protected virtual void WarnAboutNullInput()
        {
            Debug.LogWarning("ProgressMarkerCommand: Marker ID is null or empty. No action will be performed.");
        }

        public override string GetSummary()
        {
            string idVal = _markerID.Value;
            string result = $"{_action} | ID: {idVal} | Order: {_markerOrder.Value}";
            return result;
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_markerID);
            _variableDataCache.Add(_markerOrder);
        }

        protected override void AssertOwnership()
        {
            // Overridden only for testing purposes.
            Flowchart fChart = GetFlowchart();
            for (int i = 0; i < _variableDataCache.Count; i++)
            {
                var currentVarData = _variableDataCache[i] as VariableData;
                //currentVarData.Refresh();
                if (currentVarData.VarOwner == null)
                {
                    Debug.Log($"ProgressMarkerCommand: Setting VarOwner of {currentVarData} at index {i} to {fChart}");
                    currentVarData.VarOwner = fChart;
                }
                
            }
        }
    }
}