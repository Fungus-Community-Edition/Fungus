using Amanita.SaveSys.UI;
using UnityEngine;
using TMPro;

namespace Amanita.SaveSys
{
    public class SaveSlotNumberView : SaveSlotTextView
    {
        [SerializeField] protected TextMeshProUGUI slotNumberDisplay;

        protected override object WhatToFormat => Meta?.SlotNumber;
    }

    
}