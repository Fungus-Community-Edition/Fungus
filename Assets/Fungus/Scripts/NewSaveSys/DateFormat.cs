using System;
using System.Globalization;
using UnityEngine;

namespace Amanita
{
    [CreateAssetMenu(fileName = "DateFormat", menuName = "Amanita/DateFormat", order = 1)]
    public class DateFormat : ScriptableObject, IDateFormat
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

        public virtual string FormatDate(System.DateTime date)
        {
            return date.ToString(inTextForm);
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
            return inTextForm;
        }
    }

    public interface IDateFormat
    {
        string FormatDate(System.DateTime date);
    }
}