using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sets the text property on a UI Text object and/or an Input Field object.
    /// </summary>
    [CommandInfo("UI", 
                 "Set Text", 
                 "Sets the text property on a UI Text object and/or an Input Field object.")]
    [AddComponentMenu("")]
    public class SetText : Command, ILocalizable 
    {
        [Tooltip("Text object to set text on. Can be a UI Text, Text Field or Text Mesh object.")]
        [SerializeField] protected GameObjectData targetTextObject = new GameObjectData();
        
        [Tooltip("String value to assign to the text object")]
        [FormerlySerializedAs("stringData")]
        [SerializeField] protected StringDataMulti text;

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected string description;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(targetTextObject);
            variableDataCache.Add(text);
        }

        #region Public members

        public override void OnEnter()
        {
            var flowchart = GetFlowchart();
            string newText = flowchart.SubstituteVariables(text.Value);
            
            if (targetTextObject == null)
            {
                Continue();
                return;
            }

            TextAdapter textAdapter = new TextAdapter();
            textAdapter.InitFromGameObject(targetTextObject);

            if (textAdapter.HasTextObject())
            {
                textAdapter.Text = newText;
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            if (targetTextObject != null)
            {
                return targetTextObject.Value.name + " : " + text.Value;
            }
            
            return "Error: No text object selected";
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

        // Backwards compatibility with Fungus v2.1.2
        [HideInInspector]
        [FormerlySerializedAs("targetTextObject")]
        public GameObject _textObjectObsolete;
        protected override void OnEnable()
        {
            base.OnEnable();
            if (_textObjectObsolete != null)
            {
                targetTextObject.Value = _textObjectObsolete.gameObject;
            }
        }

        #endregion
    }    
}
