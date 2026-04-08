using UnityEngine;
using AtMycelia.Hyphlow;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.DialogueSys
{
    /// <summary>
    /// Text coloring mode for Write command.
    /// </summary>
    public enum TextColor
    {
        /// <summary> Don't change the text color. </summary>
        Default,
        /// <summary> Set the text alpha to 1. </summary>
        SetVisible,
        /// <summary> Set the text alpha to a value. </summary>
        SetAlpha,
        /// <summary> Set the text color to a value. </summary>
        SetColor
    }

    /// <summary>
    /// Writes content to a UI Text or Text Mesh object.
    /// </summary>
    [CommandInfo("UI", 
                 "Write", 
                 "Writes content to a UI Text or Text Mesh object.")]
    [AddComponentMenu("")]
    public class Write : Command, ILocalizable
    {
        [Tooltip("Text object to set text on. Text, Input Field and Text Mesh objects are supported.")]
        [SerializeField] protected GameObjectData textObject = new GameObjectData();

        [Tooltip("String value to assign to the text object")]
        [HyphlowTextArea(3, 10)]
        [SerializeField] protected StringDataMulti text = new StringDataMulti();

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected string description;

        [Tooltip("Clear existing text before writing new text")]
        [SerializeField] protected BooleanData clearText = new BooleanData(true);

        [Tooltip("Wait until this command finishes before executing the next command")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);

        [Tooltip("Color mode to apply to the text.")]
        [SerializeField] protected TextColor textColor = TextColor.Default;

        [Tooltip("Alpha to apply to the text.")]
        [SerializeField] protected FloatData setAlpha = new FloatData(1f);

        [Tooltip("Color to apply to the text.")]
        [SerializeField] protected ColorData setColor = new ColorData(Color.white);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(textObject);
            _variableDataCache.Add(text);
            _variableDataCache.Add(clearText);
            _variableDataCache.Add(waitUntilFinished);
            _variableDataCache.Add(setAlpha);
            _variableDataCache.Add(setColor);
        }

        protected Writer GetWriter()
        {
            var writer = textObject.GetComponent<Writer>();
            if (writer == null)
            {
                writer = textObject.AddComponent<Writer>();
            }
            
            return writer;
        }

        #region Public members

        public override void OnEnter()
        {
            if (textObject == null)
            {
                Continue();
                return;
            }
        
            var writer = GetWriter();
            if (writer == null)
            {
                Continue();
                return;
            }

            switch (textColor)
            {
            case TextColor.SetAlpha:
                writer.SetTextAlpha(setAlpha);
                break;
            case TextColor.SetColor:
                writer.SetTextColor(setColor);
                break;
            case TextColor.SetVisible:
                writer.SetTextAlpha(1f);
                break;
            }

            var flowchart = GetFlowchart();
            string newText = flowchart.SubstituteVariables(text.Value);

            if (!waitUntilFinished)
            {
                StartCoroutine(writer.Write(newText, clearText, false, true, false, null, null));
                Continue();
            }
            else
            {
                StartCoroutine(writer.Write(newText, clearText, false, true, false, null,
                             () => { Continue (); }
                ));
            }
        }

        public override string GetSummary()
        {
            if (textObject != null)
            {
                return textObject.Name + " : " + text.Value;
            }

            return "Error: No text object selected";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override void OnStopExecuting()
        {
            GetWriter().Stop();
        }

        #endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return text;
        }

        public virtual void SetStandardText(string standardText)
        {
            text.Value = standardText;
        }

        public virtual string GetDescription()
        {
            return description;
        }
        
        public virtual string GetStringId()
        {
            // String id for Write commands is WRITE.<Localization Id>.<Command id>
            return "WRITE." + GetFlowchartLocalizationId() + "." + itemId;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(text.VarRef, variable) || 
                ReferenceEquals(setAlpha.VarRef, variable) || 
                ReferenceEquals(setColor.VarRef, variable) || base.HasReference(variable);
        }

        #endregion

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldTextObject != null)
            {
                textObject.Value = _oldTextObject;
                _oldTextObject = null;
            }

            // Gotta keep in mind the defaults for these bools when deciding whether or not to
            // migrate them. If the old bool is false, that means the user had it toggled off,
            // so we should migrate that value over. If it's true, that means the user never
            // touched it and we should just keep it as is.
            if (_oldClearText == false)
            {
                clearText.Value = _oldClearText;
                _oldClearText = true;
            }

            if (_oldWaitUntilFinished == false)
            {
                waitUntilFinished.Value = _oldWaitUntilFinished;
                _oldWaitUntilFinished = true;
            }
        }

        [FormerlySerializedAs("textObject")]
        [SerializeField] [HideInInspector] protected GameObject _oldTextObject;
        [FormerlySerializedAs("clearText")]
        [SerializeField] protected bool _oldClearText = true;

        [FormerlySerializedAs("waitUntilFinished")]
        [SerializeField] protected bool _oldWaitUntilFinished = true;
    }
}
