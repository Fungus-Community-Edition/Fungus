using UnityEngine;
using TMPro;

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
            string formattedPlaytime = Meta.Playtime.ToFormattedString(playtimeFormat);
            Text = $"{prefix}{formattedPlaytime}";
        }

        public virtual string Text
        {
            get => playtimeText.text;
            protected set => playtimeText.text = value;
        }

    }
}