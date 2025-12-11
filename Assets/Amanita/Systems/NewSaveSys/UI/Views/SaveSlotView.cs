using UnityEngine;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// For displaying save slot metadata such as the time stamp, slot number, etc.
    /// </summary>
    public abstract class SaveSlotView : MonoBehaviour, ISaveSlotView
    {
        [TextArea(3, 6)]
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
            // Override in subclasses to add validation logic if needed
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