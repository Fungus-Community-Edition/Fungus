namespace Amanita
{
    public static class TimeSpanExtensions
    {
        public static string ToFormattedString(this System.TimeSpan timeSpan, PlaytimeFormatEnum format)
        {
            return format switch
            {
                PlaytimeFormatEnum.HoursMinutesSeconds => $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
                PlaytimeFormatEnum.MinutesSeconds => $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}",
                PlaytimeFormatEnum.CompactText => $"{timeSpan.Hours}h {timeSpan.Minutes}m",
                PlaytimeFormatEnum.FullText => $"{timeSpan.Hours} hours, {timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds",
                PlaytimeFormatEnum.TotalHours => $"{(int)timeSpan.TotalHours}.{timeSpan.Minutes:D2} hours",
                PlaytimeFormatEnum.Custom => "Custom format not implemented",// Custom formatting logic can be added here
                _ => "Unknown format",
            };
        }

        public static string ToString(this System.TimeSpan timeSpan, string inputStr, bool isEscaped)
        {
            string result;

            if (isEscaped)
            {
                result = timeSpan.ToString(inputStr);
            }
            else
            {
                string escapedFormat = EscapeTimeSpanFormat(inputStr);
                result = timeSpan.ToString(escapedFormat);
            }

            return result;
        }

        private static string EscapeTimeSpanFormat(string input)
        {
            // Backslashes, colons, and periods need escaping
            return input
                .Replace(@"\", @"\\")  
                .Replace(":", @"\:")  
                .Replace(".", @"\.");
        }
    }
}