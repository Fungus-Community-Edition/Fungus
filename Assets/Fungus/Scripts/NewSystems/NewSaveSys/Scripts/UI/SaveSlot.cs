using UnityEngine;
using UnityEngine.UI;
using DateTime = System.DateTime;
using System.Globalization;
using CGT.Globalization;

namespace CGTUnity.Fungus.SaveSystem
{
    [RequireComponent(typeof(RectTransform))]
    public class SaveSlot : MonoBehaviour
    {
        #region Fields
        [Tooltip("Displays the number for this slot.")]
        [SerializeField] protected Text numDisplay = null;
        [Tooltip("Displays the description for this slot.")]
        [SerializeField] protected Text descDisplay = null;
        [Tooltip("Displays the last-written date for this slot.")]
        [SerializeField] protected Text dateDisplay = null;
        [Tooltip("The .NET Standard format to display the date in.")]
        [SerializeField] protected StandardFormat dateFormat = StandardFormat.fullLongDate;
        [Tooltip("Whether or not this updates its displays every frame.")]
        [SerializeField] protected bool refreshContinuously = true;

        
        protected GameSaveData saveData = null;
        
        #endregion

        #region Properties and helpers
        #region UI Elements
        public RectTransform rectTransform                      { get; private set; }
        

        #endregion
        
        public virtual GameSaveData SaveData
        {
            get                                                 { return saveData; }
            set 
            {
                if (value == null)
                {
                    WarnForNullSaveDataAssignment();
                    return;
                }
                
                saveData = value;
                SyncWithSaveData();
                UpdateDisplays();
                Signals.SaveSlotUpdated.Invoke(this, value);
            }
        }

        void WarnForNullSaveDataAssignment()
        {
            string message =
                    @"Cannot assign null GameSaveData to a Save Slot. If you want to 
                    clear the slot, call its Clear() method instead.";
            Debug.LogWarning(message);
        }

        protected virtual void SyncWithSaveData()
        {
            Number = saveData.SlotNumber;
            Description = saveData.Description;
            Date = saveData.LastWritten;
        }

        /// <summary>
        /// Defines which slot this is in the Save Menu. First, second, etc.
        /// </summary>
        public virtual int Number { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// Updates the UI elements in this SaveSlot based on the save data (or lack thereof) it holds.
        /// </summary>
        public virtual void UpdateDisplays()
        {
            UpdateNumberDisplay();
            UpdateDateDisplay();
            UpdateDescriptionDisplay();
        }

        protected virtual void UpdateNumberDisplay()
        {
            numDisplay.text = "Save #" + Number;
        }

        protected virtual void UpdateDateDisplay()
        {
            if (Date == default(DateTime))
                // The default is so far back in the past, it can't be accurate,
                // considering when this system came out
                dateDisplay.text = "";
            else
                DisplayDateBasedOnUserLocale();
        }

        void DisplayDateBasedOnUserLocale()
        {
            string formatVal = StandardFormatValues.vals[dateFormat];
            dateDisplay.text = Date.ToString(formatVal, userLocale);
        }

        protected readonly CultureInfo userLocale = CultureInfo.CurrentUICulture;

        protected virtual void UpdateDescriptionDisplay()
        {
            descDisplay.text = Description;
        }

        #endregion

        #region Methods
        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Number = rectTransform.GetSiblingIndex();
            Description = "";
            clickReceiver = GetComponent<Button>();
            clickReceiver.onClick.AddListener(OnClick);
            UpdateDisplays();
        }

        protected Button clickReceiver = null;

        protected virtual void OnClick()
        {
            Signals.SaveSlotClicked.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            clickReceiver.onClick.RemoveListener(OnClick);
        }

        public virtual void Clear()
        {
            saveData = null;
            Description = "";
            Date = default(DateTime);
            UpdateDisplays();
            Signals.SaveSlotUpdated.Invoke(this, SaveData);
        }

        protected virtual void Update()
        {
            if (refreshContinuously)
                UpdateDisplays();
        }

        #endregion
    }
}