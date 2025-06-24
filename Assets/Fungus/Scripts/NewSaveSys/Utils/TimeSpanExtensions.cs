namespace Amanita
{
    public static class TimeSpanExtensions
    {
        public static string ToFormattedString(this System.TimeSpan timeSpan, PlaytimeFormat format)
        {
            return format switch
            {
                PlaytimeFormat.HoursMinutesSeconds => $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
                PlaytimeFormat.MinutesSeconds => $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
                PlaytimeFormat.CompactText => $"{timeSpan.Hours}h {timeSpan.Minutes}m",
                PlaytimeFormat.FullText => $"{timeSpan.Hours} hours, {timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds",
                PlaytimeFormat.TotalHours => $"{(int)timeSpan.TotalHours}.{timeSpan.Minutes:D2} hours",
                PlaytimeFormat.Custom => "Custom format not implemented",// Custom formatting logic can be added here
                _ => "Unknown format",
            };
        }
    }
}