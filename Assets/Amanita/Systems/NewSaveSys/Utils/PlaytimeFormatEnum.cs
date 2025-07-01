namespace Amanita
{
    public enum PlaytimeFormatEnum
    {
        Null,                 // Reserved for null or uninitialized state
        HoursMinutesSeconds,  // e.g. 12:34:56
        MinutesSeconds,       // e.g. 34:56
        CompactText,          // e.g. 12h 34m
        FullText,             // e.g. 12 hours, 34 minutes, 56 seconds
        TotalHours,           // e.g. 12.58 hours
        Custom                // Reserved for user-defined formatting
    }
}