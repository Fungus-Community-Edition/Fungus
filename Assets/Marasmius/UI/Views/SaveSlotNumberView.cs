using AtMycelia.SaveSys.UI;
using UnityEngine;
using TMPro;

namespace AtMycelia.SaveSys
{
    public class SaveSlotNumberView : SaveSlotTextView
    {
        [SerializeField] protected TextMeshProUGUI slotNumberDisplay;

        protected override object WhatToFormat => Meta?.SlotNumber;
    }

    
}