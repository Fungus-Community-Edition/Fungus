using System;
using UnityEngine;

namespace Amanita.SaveSys.UI
{
    [CreateAssetMenu(fileName = "NewPlaytimeFormatter", menuName = "Amanita/PlaytimeFormatter", order = 1)]
    public class PlaytimeFormatter : ScriptableObject, IPlaytimeFormatter
    {
        [TextArea(3, 10)]
        [SerializeField] protected string notes = string.Empty;
        [SerializeField] protected string inTextForm = "HH:mm:ss";
        public virtual string InTextForm
        {
            get => inTextForm;
            set
            {
                inTextForm = value;
                OnValidate();
            }
        }

        public virtual string FormatToText(object toFormat)
        {
            if (toFormat is TimeSpan playtime)
            {
                return FormatPlaytime(playtime);
            }
            else
            {
                Debug.LogWarning($"Cannot format object of type {toFormat.GetType()}. Expected TimeSpan.");
                return string.Empty;
            }
        }

        public virtual string FormatPlaytime(TimeSpan playtime)
        {
            string result = playtime.ToString(inTextForm, false);
            // ^Since TimeSpan requires characters such as the colon to be escaped in the format string,
            // unlike DateTime which can handle colons directly. Rather than escape it here, we're
            // using a custom extension method that handles escaping.
            return result;
        }

        public override string ToString()
        {
            return $"PlaytimeFormat: {inTextForm}";
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(inTextForm))
            {
                Debug.LogWarning("Playtime format string is empty or null, using default format: HH:mm:ss.");
                inTextForm = "HH:mm:ss";
            }
            // Additional validation logic can be added here if needed.
        }
    }

    /// <summary>
    /// For ScriptableObjects that format objects to text form.
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