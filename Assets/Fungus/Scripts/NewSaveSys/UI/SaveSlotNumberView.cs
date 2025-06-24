using Amanita.SaveSys.UI;
using UnityEngine;
using TMPro;

namespace Amanita.SaveSys
{
    public class SaveSlotNumberView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI slotNumberText;
        [SerializeField] protected string numberFormat = "D2"; // Default format for slot number

        public override void Refresh()
        {
            base.Refresh();
            if (Meta != null && slotNumberText != null)
            {
                slotNumberText.text = Meta.SlotNumber.ToString();
            }
            else
            {
                Debug.LogWarning("Meta data or slotNumberText is null, cannot refresh Save Slot Number View.");
            }
        }
    }
}