using UnityEngine;
using TMPro;

namespace Amanita.SaveSys.UI
{
    public class SaveSlotPlaytimeView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI playtimeText;
        [SerializeField] protected string prefix = "Playtime: ";
        [SerializeField] protected PlaytimeFormat playtimeFormat = PlaytimeFormat.HoursMinutesSeconds;

        protected override void UpdateVisuals()
        {
            string formattedPlaytime = Meta.Playtime.ToFormattedString(playtimeFormat);
            playtimeText.text = $"{prefix}{formattedPlaytime}";
        }

    }
}