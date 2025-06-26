using Amanita.SaveSys.UI;
using System;
using System.Globalization;
using UnityEngine;

namespace Amanita
{
    [CreateAssetMenu(fileName = "NewDateFormatter", menuName = "Amanita/DateFormatter", order = 1)]
    public class DateFormatter : TextFormatter, IDateFormatter
    {
        protected override string DefaultFormat => "yyyy-MM-dd HH:mm:ss";

        public override string FormatToText(object toFormat)
        {
            if (toFormat is DateTime date)
            {
                return FormatDate(date);
            }
            else
            {
                Debug.LogWarning($"Cannot format object of type {toFormat.GetType()}. Expected DateTime.");
                return string.Empty;
            }
        }

        public virtual string FormatDate(System.DateTime date)
        {
            string dateString = date.ToString(FormatString);
            string result = $"{Prefix}{dateString}{postfix}";
            return result;
        }

        protected override void OnValidate()
        {
            if (string.IsNullOrEmpty(formatString))
            {
                Debug.LogWarning($"Date format string is empty or null, using default format: {DefaultFormat}.");
                formatString = DefaultFormat;
            }

            ValidateDateFormat();
            void ValidateDateFormat()
            {
                try
                {
                    string sample = DateTime.Now.ToString(formatString); // Throws if format is totally invalid
                    bool valid = DateTime.TryParseExact(
                        sample,
                        formatString,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _
                    );

                    if (!valid)
                    {
                        throw new FormatException();
                    }
                }
                catch (FormatException)
                {
                    Debug.LogWarning($"Invalid date format: {formatString}. Using default format: {DefaultFormat}.");
                    formatString = DefaultFormat;
                }
            }

        }

        public override string ToString()
        {
            return $"DateFormat: {formatString}";
        }
    }

    public interface IDateFormatter : ISlotUITextFormatter
    {
        string FormatDate(DateTime date);
    }

}