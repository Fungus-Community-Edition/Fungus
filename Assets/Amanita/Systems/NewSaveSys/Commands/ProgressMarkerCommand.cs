using Amanita.VScripting;
using UnityEngine;
using System.Collections.Generic;

namespace Amanita.SaveSys.VScripting
{
    [CommandInfo("Save Sys", 
        "Progress Marker", 
        "Marks a point in the game's progress for save/load purposes.")]
    public class ProgressMarkerCommand : Command
    {
        [SerializeField] protected PMCAction action = PMCAction.Register;
        [SerializeField] protected StringData markerID = new StringData("DefaultMarker");
        [Tooltip("Determines the order of this marker relative to others. " +
            "Lower numbers indicate earlier execution in SaveDataLoaded events.")]
        [SerializeField] protected IntegerData markerOrder = new IntegerData(0);
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
            var handler = actionHandlers[action];
            handler?.Invoke();
            Continue();
        }

        protected virtual void HandleRegistration()
        {
            string id = markerID.Value;
            int order = markerOrder.Value;
            SaveSys.RegisterProgressMarker(id, order);
        }

        protected SaveSystem SaveSys
        {
            get { return SaveSystem.S; }
        }

        protected virtual void HandleDeregistration()
        {
            string id = markerID.Value;
            SaveSys.UnregisterProgressMarker(id);
        }

        protected virtual void HandleSettingOrder()
        {
            string id = markerID.Value;
            int order = markerOrder.Value;
            SaveSys.SetProgressMarkerOrder(id, order);
        }

        protected virtual void WarnAboutNullInput()
        {
            Debug.LogWarning("ProgressMarkerCommand: Marker ID is null or empty. No action will be performed.");
        }

        public override string GetSummary()
        {
            string idVal = markerID.Value;
            string result = $"{action} | ID: {idVal} | Order: {markerOrder.Value}";
            return result;
        }

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(markerID);
            variableDataCache.Add(markerOrder);
        }

        protected override void AssertOwnership()
        {
            // Overridden only for testing purposes.
            Flowchart fChart = GetFlowchart();
            for (int i = 0; i < variableDataCache.Count; i++)
            {
                var currentVarData = variableDataCache[i] as VariableData;
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