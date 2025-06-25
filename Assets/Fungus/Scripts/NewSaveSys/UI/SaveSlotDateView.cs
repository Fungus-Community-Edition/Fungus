using UnityEngine;
using TMPro;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotDateView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI dateDisplay;
        [SerializeField] protected ScriptableObject dateFormatSO;
        
        protected virtual void Awake()
        {
            DateFormat = (IDateFormat)dateFormatSO;

            if (dateDisplay == null)
            {
                string errorMessage = "Date Display is not assigned.";
                Debug.LogError(errorMessage);
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            if (Meta != null && dateDisplay != null)
            {
                dateDisplay.text = DateFormat.FormatDate(Meta.TimeStamp);
            }
            else
            {
                Debug.LogWarning("Meta data or dateText is null. Cannot refresh Save Slot Date View.");
            }
        }

        public IDateFormat DateFormat
        {
            get => dateFormat;
            set => dateFormat = value;
        }

        protected IDateFormat dateFormat;

    }

    
}