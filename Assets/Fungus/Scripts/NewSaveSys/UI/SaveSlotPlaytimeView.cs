using UnityEngine;
using TMPro;
using System;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotPlaytimeView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI playtimeText;
        [SerializeField] protected string prefix = "Playtime: ";
        [SerializeField] protected PlaytimeFormat playtimeFormat = PlaytimeFormat.HoursMinutesSeconds;

        public virtual string Prefix
        {
            get => prefix;
            set
            {
                if (prefix != value)
                {
                    prefix = value;
                    UpdateVisuals();
                }
            }
        }

        public virtual PlaytimeFormat Format
        {
            get => playtimeFormat;
            set
            {
                playtimeFormat = value;
                UpdateVisuals();
            }
        }

        protected override void UpdateVisuals()
        {
            string formattedPlaytime = PlayTime.ToFormattedString(playtimeFormat);
            Text = $"{prefix}{formattedPlaytime}";
        }

        protected virtual TimeSpan PlayTime
        {
            get
            {
                if (Meta != null)
                {
                    return Meta.Playtime;
                }
                else
                {
                    Debug.LogWarning("Meta data is null. Cannot retrieve playtime.");
                    return TimeSpan.Zero;
                }
            }
        }

        public virtual string Text
        {
            get => playtimeText.text;
            protected set => playtimeText.text = value;
        }

    }
}