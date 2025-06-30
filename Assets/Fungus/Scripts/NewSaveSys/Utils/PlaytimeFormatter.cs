using System;
using UnityEngine;

namespace Amanita.UI
{
    [CreateAssetMenu(fileName = "NewPlaytimeFormatter", menuName = "Amanita/UI/Formatters/PlaytimeFormatter", order = 1)]
    public class PlaytimeFormatter : TextFormatter, IPlaytimeFormatter
    {
        protected override bool CanWorkWith(object toFormat)
        {
            return toFormat is TimeSpan;
        }

        protected override string FormatAsAppropriate(object toFormat)
        {
            return FormatPlaytime((TimeSpan)toFormat);
        }

        public virtual string FormatPlaytime(TimeSpan playtime)
        {
            string playtimeString = playtime.ToString(formatString, false);
            // ^Since TimeSpan requires characters such as the colon to be escaped in the format string,
            // unlike DateTime which can handle colons directly. Rather than escape it here, we're
            // using a custom extension method that handles it for us.
            string result = $"{Prefix}{playtimeString}{postfix}";
            
            return result;
        }

        public override string ToString()
        {
            return $"PlaytimeFormat: {formatString}";
        }

        protected override string DefaultFormat => "hh:mm:ss";
        // ^Note that we avoid using a capital H since TimeSpan doesn't like that

        protected override void OnValidate()
        {
            base.OnValidate();

            ValidatePlaytimeFormat();
            void ValidatePlaytimeFormat()
            {
                try
                {
                    string sample = TimeSpan.FromHours(1).ToString(formatString, false);
                    // ^Throws if format is totally invalid
                    // No further validation needed since TimeSpan.ToString() handles the format string correctly.
                }
                catch (FormatException)
                {
                    Debug.LogWarning($"Invalid playtime format: {formatString}. Using default format: {DefaultFormat}.");
                    formatString = DefaultFormat;
                }
            }
        }
    }

    /// <summary>
    /// For ScriptableObjects that format objects to text form based on their own criteria.
    /// </summary>
    public interface ITextFormatter
    {
        string FormatToText(System.Object toFormat);
    }

    public interface IPlaytimeFormatter : ITextFormatter
    {
        string FormatPlaytime(System.TimeSpan playtime);
    }
}