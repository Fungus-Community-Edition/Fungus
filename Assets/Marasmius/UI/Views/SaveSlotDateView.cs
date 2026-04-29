namespace AtMycelia.SaveSys.Ui
{
    public class SaveSlotDateView : SaveSlotTextView
    {
        protected override object WhatToFormat => Meta?.TimeStamp;

    }

}