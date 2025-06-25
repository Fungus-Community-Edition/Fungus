using Amanita.SaveSys.UI;
using UnityEngine;
using TMPro;
using System;

namespace Amanita.SaveSys
{
    public class SaveSlotNumberView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI slotNumberDisplay;
        [SerializeField] protected string prefix = "Slot # ";
        [SerializeField] protected SlotNumFormat format = SlotNumFormat.PaddedTwoDigits;

        public virtual string Prefix
        {
            get => prefix;
            set
            {
                prefix = value;
                UpdateVisuals();
            }
        }

        public virtual SlotNumFormat Format
        {
            get => format;
            set
            {
                if (format != value)
                {
                    format = value;
                    UpdateVisuals();
                }
            }
        }

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            bool weHaveAllWeNeed = Meta != null && slotNumberDisplay != null;

            if (!weHaveAllWeNeed)
            {
                Debug.LogWarning("Meta data or slotNumberText is null. Cannot refresh Save Slot Number View.");
                return;
            }

            string numStr = string.Empty, errorMessage = string.Empty;
            NotImplementedException theFactThatSomethingWentWrong;

            switch (format)
            {
                case SlotNumFormat.PaddedTwoDigits:
                    numStr = Meta.SlotNumber.ToString("D2");
                    break;
                case SlotNumFormat.Ordinal:
                    numStr = meta.SlotNumber.ToString();
                    break;
                case SlotNumFormat.RomanNumeral:
                    numStr = RomanNumeralConverter.ToRoman(Meta.SlotNumber);
                    break;
                case SlotNumFormat.Custom:
                    errorMessage = "If you want custom save slot num formatting, "
                        + "best implement your own solution instead of using the "
                        + "built-in SaveSlotNumberView class.";
                    break;
                case SlotNumFormat.Null:
                    errorMessage = "Null format is fake news and only there for "
                        + "programmers' convenience. You need to use another "
                        + "one, believe me.";
                    break;

            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                theFactThatSomethingWentWrong = new NotImplementedException(errorMessage);
                throw theFactThatSomethingWentWrong;
            }

            Text = $"{prefix}{numStr}";
            
        }

        public virtual string Text
        {
            get => slotNumberDisplay.text;
            protected set
            {
                slotNumberDisplay.text = value;
            }
        }
    }

    
}