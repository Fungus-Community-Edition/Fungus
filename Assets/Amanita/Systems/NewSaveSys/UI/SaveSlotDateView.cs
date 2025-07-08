namespace Amanita.SaveSys.UI
{
    public class SaveSlotDateView : SaveSlotTextView
    {
        protected override object WhatToFormat => Meta?.TimeStamp;

    }

}