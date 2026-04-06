namespace AtMycelia.SaveSys.UI
{
    public class SaveSlotPlaytimeView : SaveSlotTextView
    {
        protected override object WhatToFormat => Meta?.Playtime;

    }
}