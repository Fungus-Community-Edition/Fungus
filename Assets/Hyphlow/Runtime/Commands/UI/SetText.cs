using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets the text property on a UI Text object and/or an Input Field object.
    /// </summary>
    [CommandInfo("UI", 
                 "Set Text", 
                 "Sets the text property on a UI Text object and/or an Input Field object.")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetText : Command
    {
        [Tooltip("Text object to set text on. Can be a UI Text, Text Field or Text Mesh object.")]
        [SerializeField] protected GameObjectData _targetTextObject = new GameObjectData();
        
        [Tooltip("String value to assign to the text object")]
        [FormerlySerializedAs("stringData")]
        [SerializeField] protected StringDataMulti text = new StringDataMulti();

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [HyphlowTextArea(3, 10)]
        [SerializeField] protected string description;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetTextObject);
            _variableDataCache.Add(text);
        }

        #region Public members

        public override void OnEnter()
        {
            var flowchart = GetFlowchart();
            string newText = flowchart.SubstituteVariables(text.Value);
            
            if (_targetTextObject == null)
            {
                Continue();
                return;
            }

            TextAdapter textAdapter = new TextAdapter();
            textAdapter.InitFromGameObject(_targetTextObject);

            if (textAdapter.HasTextObject())
            {
                textAdapter.Text = newText;
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            string result = "Error: No text object selected";
            if (_targetTextObject != null && _targetTextObject.Value != null)
            {
                string textSummary = GetTextSummaryStr();
                result = $"{_targetTextObject.Value.name} to {textSummary}";
            }
            
            return result;
        }

        private string GetTextSummaryStr()
        {
            if (text == null || (string.IsNullOrEmpty(text.Value) && !text.RepresentingVar))
            {
                return "None";
            }

            if (text.RepresentingVar)
            {
                return text.VarRef.Key;
            }
            else
            {
                return $"\"{text.Value}\"";
            }
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(text.VarRef, variable) || base.HasReference(variable);
        }

        #endregion


        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            var f = GetFlowchart();
            f.DetermineSubstituteVariables(text, referencedVariables);
        }
#endif
        #endregion Editor caches

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
            // String id for Set Text commands is SETTEXT.<Localization Id>.<Command id>
            return "SETTEXT." + GetFlowchartLocalizationId() + "." + itemId;
        }

        #endregion

        #region Backwards compatibility

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();

            if (targetTextObject != null)
            {
                _targetTextObject.Value = targetTextObject;
                targetTextObject = null;
            }
        }

        [SerializeField]
        [HideInInspector]
        protected GameObject targetTextObject;

        #endregion
    }    
}
