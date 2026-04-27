namespace AtMycelia.SaveSys.Ui
{
    public class SaveSlotPlaytimeView : SaveSlotTextView
    {
        protected override object WhatToFormat => Meta?.Playtime;

    }
}