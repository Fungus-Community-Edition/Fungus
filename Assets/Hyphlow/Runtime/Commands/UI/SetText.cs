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
        [FormerlySerializedAs("text")]
        [HyphlowTextArea(3, 10)]
        [SerializeField] protected StringDataMulti _text = new StringDataMulti();

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [HyphlowTextArea(3, 10)]
        [SerializeField] protected StringData _description;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetTextObject);
            _variableDataCache.Add(_text);
        }

        #region Public members

        public override void OnEnter()
        {
            var flowchart = GetFlowchart();
            string newText = flowchart.SubstituteVariables(_text.Value);
            
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
            if (_text == null || (string.IsNullOrEmpty(_text.Value) && !_text.RepresentingVar))
            {
                return "None";
            }

            if (_text.RepresentingVar)
            {
                return _text.VarRef.Key;
            }
            else
            {
                return $"\"{_text.Value}\"";
            }
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_text.VarRef, variable) || base.HasReference(variable);
        }

        #endregion


        #region Editor caches
#if UNITY_EDITOR
        protected override void RefreshVariableCache()
        {
            base.RefreshVariableCache();

            var f = GetFlowchart();
            f.DetermineSubstituteVariables(_text, referencedVariables);
        }
#endif
        #endregion Editor caches

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return _text;
        }

        public virtual void SetStandardText(string standardText)
        {
            _text.Value = standardText;
        }

        public virtual string GetDescription()
        {
            return _oldDescription;
        }
        
        public virtual string GetStringId()
        {
            // String id for Set Text commands is SETTEXT.<Localization Id>.<Command id>
            return "SETTEXT." + "." + itemId;
        }

        #endregion

        #region Backwards compatibility

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();

            if (_oldTargetTextObject != null)
            {
                _targetTextObject.Value = _oldTargetTextObject;
                _oldTargetTextObject = null;
            }

            if (!string.IsNullOrEmpty(_oldDescription))
            {
                _description.Value = _oldDescription;
                _oldDescription = null;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("targetTextObject")]
        [HideInInspector]
        protected GameObject _oldTargetTextObject;

        [SerializeField]
        [FormerlySerializedAs("description")]
        [HideInInspector]
        protected string _oldDescription;

        #endregion
    }    
}
