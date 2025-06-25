using UnityEngine;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// For displaying save slot metadata such as the time stamp, slot number, etc.
    /// </summary>
    public abstract class SaveSlotUIView : MonoBehaviour, ISaveSlotUIView
    {
        [TextArea(3, 10)]
        [SerializeField] protected string notes = string.Empty;

        public virtual ISaveMetaData Meta
        {
            get { return meta; }
            set
            {
                meta = value;
                ValidateMeta();
                Refresh();
            }
        }
        protected ISaveMetaData meta;

        protected virtual void ValidateMeta()
        {
            if (Meta == null)
            {
                Debug.LogError("Meta data is null. Please assign a valid ISaveMetaData instance.");
            }
            else
            {
                Debug.Log($"Meta data is valid: {Meta.SlotNumber} - {Meta.TimeStamp}");
            }
        }

        public virtual void Refresh()
        {
            // Implement the logic to refresh the UI with the current metadata
            if (Meta != null)
            {
                // Example: Update UI elements with Meta data
                Debug.Log($"Refreshing Save Slot View: {Meta.SlotNumber} - {Meta.TimeStamp}");
                UpdateVisuals();
            }
            else
            {
                Debug.LogWarning("Meta data is null, cannot refresh Save Slot View.");
            }
        }

        protected virtual void UpdateVisuals()
        {
            // We assume that the meta is valid here.

        }

    }
}