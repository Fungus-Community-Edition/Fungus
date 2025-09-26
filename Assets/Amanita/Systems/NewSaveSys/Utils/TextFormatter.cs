using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.UI
{
    public abstract class TextFormatter : ScriptableObject, ITextFormatter
    {
        [TextArea(3, 10)]
        [SerializeField] protected string notes = string.Empty;
        [SerializeField] protected string prefix = string.Empty;
        [FormerlySerializedAs("inTextForm")]
        [SerializeField] protected string formatString = string.Empty;
        [SerializeField] protected string postfix = string.Empty;

        public virtual string Prefix
        {
            get => prefix;
            set => prefix = value;
        }

        public virtual string FormatString
        {
            get => formatString;
            set
            {
                formatString = value;
                OnValidate();
            }
        }

        public virtual string Postfix
        {
            get => postfix;
            set => postfix = value;
        }

        public virtual string FormatToText(object toFormat)
        {
            string result = string.Empty;

            if (!CanWorkWith(toFormat))
            {
                Debug.LogWarning($"Cannot format object of type {toFormat.GetType()}. Expected a compatible type.");
                return result;
            }

            result = FormatAsAppropriate(toFormat);

            return result;
        }

        protected virtual bool CanWorkWith(object toFormat)
        {
            // This method can be overridden to provide specific type checks
            return toFormat != null;
        }

        /// <summary>
        /// When this is called, we assume that the object is of a type that this formatter can handle.
        /// </summary>
        protected abstract string FormatAsAppropriate(object toFormat);

        protected virtual void OnValidate()
        {
            // We expect subclasses to override this
            if (string.IsNullOrEmpty(formatString))
            {
                Debug.LogWarning($"Format string is empty or null. Using default: {DefaultFormat}");
                formatString = DefaultFormat;
            }
        }

        protected abstract string DefaultFormat { get; }
    }
}