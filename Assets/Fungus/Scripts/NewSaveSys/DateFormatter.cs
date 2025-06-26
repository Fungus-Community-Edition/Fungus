using Amanita.SaveSys.UI;
using System;
using System.Globalization;
using UnityEngine;

namespace Amanita
{
    [CreateAssetMenu(fileName = "NewDateFormatter", menuName = "Amanita/DateFormatter", order = 1)]
    public class DateFormatter : ScriptableObject, IDateFormatter, ITextFormatter
    {
        [SerializeField] protected string inTextForm = "yyyy-MM-dd HH:mm:ss";

        public virtual string InTextForm
        {
            get => inTextForm;
            set
            {
                inTextForm = value;
                OnValidate();
            }
        }

        protected static readonly string defaultDateFormat = "yyyy-MM-dd HH:mm:ss";

        public virtual string FormatToText(object toFormat)
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
            string result = date.ToString(InTextForm);
            return result;
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(inTextForm))
            {
                Debug.LogWarning("Date format string is empty or null, using default format: yyyy-MM-dd HH:mm:ss.");
                inTextForm = "yyyy-MM-dd HH:mm:ss";
            }

            ValidateDateFormat();
            void ValidateDateFormat()
            {
                try
                {
                    string sample = DateTime.Now.ToString(inTextForm); // Throws if format is totally invalid
                    bool valid = DateTime.TryParseExact(
                        sample,
                        inTextForm,
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
                    Debug.LogWarning($"Invalid date format: {inTextForm}. Using default format: {defaultDateFormat}.");
                    inTextForm = defaultDateFormat;
                }
            }


        }

        public override string ToString()
        {
            return $"DateFormat: {inTextForm}";
        }
    }

    public interface IDateFormatter : ITextFormatter
    {
        string FormatDate(DateTime date);
    }

}