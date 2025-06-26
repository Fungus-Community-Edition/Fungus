using System;
using UnityEngine;

namespace Amanita.SaveSys.UI
{
    [CreateAssetMenu(fileName = "NewPlaytimeFormatter", menuName = "Amanita/PlaytimeFormatter", order = 1)]
    public class PlaytimeFormatter : TextFormatter, IPlaytimeFormatter
    {
        public override string FormatToText(object toFormat)
        {
            string result = string.Empty;

            if (toFormat is TimeSpan playtime)
            {
                result = FormatPlaytime(playtime);
            }
            else
            {
                Debug.LogWarning($"Cannot format object of type {toFormat.GetType()}. Expected TimeSpan.");
            }

            return result;
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
    }

    /// <summary>
    /// For ScriptableObjects that format objects to text form.
    /// </summary>
    public interface ISlotUITextFormatter
    {
        string FormatToText(System.Object toFormat);
    }


    public interface IPlaytimeFormatter : ISlotUITextFormatter
    {
        string FormatPlaytime(System.TimeSpan playtime);
    }
}