using UnityEngine;
using Amanita.VScripting.EventHandlers;
using Amanita.VScripting;
using System.Linq;
using System.Collections.Generic;
using System;
using VSEvent = Amanita.VScripting.EventHandlers.EventHandler;

namespace Amanita.SaveSys.VScripting
{
    [EventHandlerInfo("SaveSys",
        "Save Loaded",
        "Triggered when a save file has been successfully loaded. This executes before or after other" +
        "Save Loaded blocks depending on the marker's order. Lower number = earlier execution")]
    public class SaveLoadedEvent : VSEvent
    {
        [ContentTypeConstraint(typeof(string))]
        [SerializeField] protected List<VariableReference> markerIDs = new List<VariableReference>();

        [Tooltip("If enabled, this event will respond to any save load regardless of marker ID.")]
        [SerializeField] protected bool respondToAny = false;

        public virtual IList<string> MarkerIDs
        {
            get
            {
                if (markerIDs == null)
                {
                    return Array.Empty<string>();
                }

                return markerIDs.Select(elem => elem.GetValue<string>())
                    .ToList()
                    .AsReadOnly();
            }
        }

        public virtual bool RespondToAny
        {
            get { return respondToAny; }
            set { respondToAny = value; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            AssertOwnership();
        }

        protected virtual void AssertOwnership()
        {
            foreach (var idRef in markerIDs)
            {
                if (idRef != null)
                {
                    idRef.VarOwner = fChart;
                }
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            AssertOwnership();
        }

        public virtual bool HasAnyRegisteredIDs()
        {
            SaveSystem saveSys = SaveSystem.S;
            var registeredIDs = saveSys.ProgressMarkers.Select(elem => elem.Id).ToHashSet();

            return markerIDs != null && markerIDs.Any(elem => registeredIDs.Contains(elem.GetValue<string>()));
        }

        /// <summary>
        /// Get the lowest order value among the registered marker IDs for this event.
        /// If this has no markers, returns int.MaxValue.
        /// </summary>
        public virtual int LowestOrder()
        {
            if (markerIDs == null || markerIDs.Count == 0)
                return int.MaxValue;

            int result = int.MaxValue;

            SaveSystem saveSys = SaveSystem.S;
            for (int i = 0; i < markerIDs.Count; i++)
            {
                VariableReference currentRef = markerIDs[i];

                ProgressMarker marker = saveSys.GetProgressMarkerByID(currentRef.GetValue<string>());
                if (marker != null && marker.Order < result)
                {
                    result = marker.Order;
                }
            }

            return result;
        }

        public virtual bool IsAbleToRespond
        {
            get
            {
                return respondToAny || HasAnyRegisteredIDs();
            }
        }

#if UNITY_EDITOR
        public virtual void AddMarkerIDVariable(IVariable<string> var)
        {
            markerIDs ??= new List<VariableReference>();
            VariableReference varRef = new VariableReference();
            varRef.VarOwner = fChart;
            varRef.Variable = var;
            markerIDs.Add(varRef);
        }
#endif
    }
}