using Amanita.SaveSys.UI;
using UnityEngine;
using TMPro;

namespace Amanita.SaveSys
{
    public class SaveSlotNumberView : SaveSlotUIView
    {
        [SerializeField] protected TextMeshProUGUI slotNumberText;
        [SerializeField] protected string prefix = "Slot # "; 
        [SerializeField] protected string numberFormat = "D2";

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            if (Meta != null && slotNumberText != null)
            {
                string numStr = Meta.SlotNumber.ToString(numberFormat);
                slotNumberText.text = $"{prefix}{numStr}";
            }
            else
            {
                Debug.LogWarning("Meta data or slotNumberText is null. Cannot refresh Save Slot Number View.");
            }
        }
    }
}