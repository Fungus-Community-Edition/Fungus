using UnityEngine;
using Amanita.VScripting.EventHandlers;
using Amanita.VScripting;
using System.Linq;
using System.Collections.Generic;

namespace Amanita.SaveSys.VScripting
{
    [EventHandlerInfo("SaveSys",
        "Save Loaded",
        "Triggered when a save file has been successfully loaded. This executes before or after other" +
        "Save Loaded blocks depending on the marker's order. Lower number = earlier execution")]
    public class SaveLoadedEvent : EventHandler
    {
        [VariableProperty(typeof(StringVariable), typeof(StringMuscariable))]
        [SerializeReference] protected List<IVariable<string>> markerIDs = new List<IVariable<string>>();

        [Tooltip("If enabled, this event will respond to any save load regardless of marker ID.")]
        [SerializeField] protected bool respondToAny = false;

        public virtual IList<string> MarkerIDs
        {
            get
            {
                if (markerIDs == null)
                {
                    return new string[0] { };
                }

                return markerIDs.Select(elem => elem.Value).ToArray();
            }
        }

        public virtual bool RespondToAny
        {
            get { return respondToAny; }
            set { respondToAny = value; }
        }

        public virtual bool HasAnyRegisteredIDs()
        {
            SaveSystem saveSys = SaveSystem.S;
            var registeredIDs = saveSys.ProgressMarkers.Select(elem => elem.Id);
            return markerIDs != null && markerIDs.Any((elem) => registeredIDs.Contains(elem.Value));
        }

#if UNITY_EDITOR
        public virtual void AddMarkerIDVariable(IVariable<string> var)
        {
            markerIDs ??= new List<IVariable<string>>();
            markerIDs.Add(var);
        }
#endif
    }
}