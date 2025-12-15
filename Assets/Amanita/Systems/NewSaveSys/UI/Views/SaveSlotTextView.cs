using TMPro;
using UnityEngine;
using Amanita.UI;

namespace Amanita.SaveSys.UI
{
    /// <summary>
    /// Base class for text-based save slot views.
    /// </summary>
    /// <remarks>
    /// This class is intended to be extended for specific text-based save slot views.
    /// </remarks>
    public abstract class SaveSlotTextView : SaveSlotView
    {
        [SerializeField] protected TextMeshProUGUI textDisplay;
        [SerializeField] protected string prefix = string.Empty;
        [SerializeField] protected string postfix = string.Empty;
        [SerializeField] protected ScriptableObject formatterSO;

        protected virtual void Awake()
        {
            CacheComponentsOnAwake();
            ValidateOnAwake();
        }

        protected virtual void CacheComponentsOnAwake()
        {
            formatter = formatterSO as ITextFormatter;
        }

        public virtual ITextFormatter Formatter
        {
            get => formatter;
            set
            {
                formatter = value;
                Refresh();
            }
        }

        protected ITextFormatter formatter;

        protected virtual void ValidateOnAwake()
        {
            if (textDisplay == null)
            {
                Debug.LogError("Text Display is not assigned.");
            }

            if (Formatter == null)
            {
                Debug.LogWarning("Formatter is not assigned.");
            }
        }

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();
            string newText = Text;
            if (Meta != null && (Meta.IsValid || acceptInvalidMeta))
            {
                string formattedObjectAsText = formatter.FormatToText(WhatToFormat);
                newText = $"{prefix}{formattedObjectAsText}{Postfix}";
            }
            else if (!acceptInvalidMeta)
            {
                newText = string.Empty;
            }

            Text = newText;
        }

        public virtual string Text
        {
            get
            {
                if (textDisplay != null)
                {
                    return textDisplay.text;
                }
                else
                {
                    Debug.LogWarning("Text Display is null. Returning empty string.");
                    return string.Empty;
                }
            }
            set
            {
                if (textDisplay != null)
                {
                    textDisplay.text = value;
                }
                else
                {
                    Debug.LogWarning("Text Display is null. Cannot set text.");
                }
            }
        }

        protected abstract System.Object WhatToFormat { get; }

        public virtual string Prefix
        {
            get => prefix;
            set
            {
                if (prefix != value)
                {
                    prefix = value;
                    UpdateVisuals();
                }
            }
        }

        public virtual string Postfix
        {
            get => postfix;
            set
            {
                if (postfix != value)
                {
                    postfix = value;
                    UpdateVisuals();
                }
            }
        }
        protected virtual void OnValidate()
        {
            bool invalidFormatterAssigned = formatter != null && formatterSO is not ITextFormatter;
            if (invalidFormatterAssigned)
            {
                Debug.LogError($"FormatterSO assigned to {this.name} does not implement ITextFormatter. "
                    + "Please assign one that does.");
            }
        }
    }
}