using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Save data type for containing an active progress markers at the time of saving.
    /// </summary>
    [System.Serializable]
    public class ProgressMarkerSaveData : SaveData
    {
        [SerializeField] ProgressMarker marker;

        public virtual ProgressMarker Marker
        {
            get { return marker; }
            set { marker = value; }
        }

        public ProgressMarkerSaveData()
        {
        }
    }
}