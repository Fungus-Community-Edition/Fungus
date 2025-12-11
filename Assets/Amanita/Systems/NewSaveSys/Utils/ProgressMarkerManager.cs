using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Amanita.SaveSys
{
    public class ProgressMarkerManager : IProgressMarkerManager
    {
        // Dictionary for O(1) lookups by ID
        protected readonly Dictionary<string, ProgressMarker> progressMarkers
            = new Dictionary<string, ProgressMarker>();

        public virtual void RegisterProgressMarker(string id, int order = 0)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot register a null or empty ProgressMarker ID.");
                return;
            }

            if (progressMarkers.ContainsKey(id))
            {
                Debug.LogWarning($"ProgressMarker with ID '{id}' is already registered.");
                return;
            }

            progressMarkers[id] = new ProgressMarker(id, order);
        }

        public virtual void UnregisterProgressMarker(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot unregister a null or empty ProgressMarker ID.");
            }
            else
            {
                bool successfullyRemoved = progressMarkers.Remove(id);
                if (!successfullyRemoved)
                {
                    Debug.LogWarning($"No ProgressMarker with ID '{id}' found to unregister.");
                }
            }

        }

        public virtual IList<ProgressMarker> ProgressMarkers
        {
            get { return progressMarkers.Values.ToList(); }
        }

        public virtual ProgressMarker GetProgressMarkerByID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            progressMarkers.TryGetValue(id, out var marker);
            return marker;
        }

        public virtual void ClearProgressMarkers()
        {
            progressMarkers.Clear();
        }

        public virtual void SetProgressMarkerOrder(string id, int newOrder)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot set order for a null or empty ProgressMarker ID.");
                return;
            }

            if (progressMarkers.TryGetValue(id, out var marker))
            {
                marker.Order = newOrder;
            }
            else
            {
                Debug.LogWarning($"No ProgressMarker with ID '{id}' found to set order. Creating new one.");
                RegisterProgressMarker(id, newOrder);
            }
        }

        public virtual bool IsProgressMarkerRegistered(string id)
        {
            return !string.IsNullOrEmpty(id) && progressMarkers.ContainsKey(id);
        }

        // Handy helper for ordered execution
        public virtual IEnumerable<ProgressMarker> GetOrderedMarkers()
        {
            return progressMarkers.Values.OrderBy(elem => elem.Order);
        }
    }

    public interface IProgressMarkerManager
    {
        void RegisterProgressMarker(string id, int order = 0);

        /// <summary>
        /// Unregisters a progress marker by its ID. If the attempt was successful, returns true.
        /// Otherwise, false.
        /// </summary>
        void UnregisterProgressMarker(string id);
        IList<ProgressMarker> ProgressMarkers { get; }
        ProgressMarker GetProgressMarkerByID(string id);
        void ClearProgressMarkers();
        void SetProgressMarkerOrder(string id, int order);
        bool IsProgressMarkerRegistered(string id);
        IEnumerable<ProgressMarker> GetOrderedMarkers();
    }
}