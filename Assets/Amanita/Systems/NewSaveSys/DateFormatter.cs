using System;
using System.Globalization;
using UnityEngine;

namespace Amanita.UI
{
    [CreateAssetMenu(fileName = "NewDateFormatter", menuName = "Amanita/UI/Formatters/DateFormatter", order = 1)]
    public class DateFormatter : TextFormatter, IDateFormatter
    {
        protected override string DefaultFormat => "yyyy-MM-dd HH:mm:ss";

        protected override bool CanWorkWith(object toFormat)
        {
            return toFormat is DateTime;
        }

        protected override string FormatAsAppropriate(object toFormat)
        {
            return FormatDate((DateTime)toFormat);
        }

        public virtual string FormatDate(System.DateTime date)
        {
            string dateString = date.ToString(FormatString);
            string result = $"{Prefix}{dateString}{postfix}";
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

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
                    formatString = DefaultFormat;
                }
            }

        }

        public override string ToString()
        {
            return $"DateFormat: {formatString}";
        }
    }

    public interface IDateFormatter : ITextFormatter
    {
        string FormatDate(DateTime date);
    }

}