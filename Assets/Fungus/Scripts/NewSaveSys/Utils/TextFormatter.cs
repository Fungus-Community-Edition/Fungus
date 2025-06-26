using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.SaveSys.UI
{
    public abstract class TextFormatter : ScriptableObject, ISlotUITextFormatter
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

        public abstract string FormatToText(object toFormat);

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