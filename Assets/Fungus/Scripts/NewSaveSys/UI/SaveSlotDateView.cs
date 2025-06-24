using UnityEngine;
using TMPro;
using System;
using System.Globalization;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotDateView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI dateText;
        [SerializeField] protected string dateFormat = "yyyy-MM-dd HH:mm:ss";
        
        public override void Refresh()
        {
            base.Refresh();
            if (Meta != null && dateText != null)
            {
                dateText.text = Meta.TimeStamp.ToString(dateFormat);
            }
            else
            {
                Debug.LogWarning("Meta data or dateText is null, cannot refresh Save Slot Date View.");
            }
        }

        protected virtual void OnValidate()
        {
            EnsureWeHaveDateText();
            void EnsureWeHaveDateText()
            {
                if (dateText == null)
                {
                    dateText = GetComponentInChildren<TextMeshProUGUI>();
                }

                bool stillGotNothing = dateText == null;
                if (stillGotNothing)
                {
                    Debug.LogError("Date Text component is not assigned or found in children.");
                }
            }
            
            ValidateDateFormat();
            void ValidateDateFormat()
            {
                bool formatIsValid = !string.IsNullOrEmpty(dateFormat) &&
                DateTime.TryParseExact("2023-01-01", dateFormat,
                null, DateTimeStyles.None, out _);
                if (!formatIsValid)
                {
                    Debug.LogWarning($"Invalid date format: {dateFormat}. Using default format: {defaultDateFormat}.");
                    dateFormat = defaultDateFormat;
                }
            }
            
        }

        protected static string defaultDateFormat = "yyyy-MM-dd HH:mm:ss";
    }
    
}