using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Gets the text property from a UI Text object and stores it in a string variable.
    /// </summary>
    [CommandInfo("UI", 
                 "Get Text", 
                 "Gets the text property from a UI Text object and stores it in a string variable.")]
    [AddComponentMenu("")]
    public class GetText : Command 
    {
        [Tooltip("Text object to get text value from")]
        [SerializeField] protected GameObjectData targetTextObject = new GameObjectData();

        [Tooltip("String variable to store the text value in")]
        [ContentTypeConstraint(typeof(string))]
        [SerializeField] protected VariableReference stringVariable = new VariableReference();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(targetTextObject);
        }

        #region Public members

        public override void OnEnter()
        {
            if (stringVariable == null)
            {
                Continue();
                return;
            }

            TextAdapter textAdapter = new TextAdapter();
            textAdapter.InitFromGameObject(targetTextObject);

            if (textAdapter.HasTextObject())
            {
                stringVariable.SetValue(textAdapter.Text);
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            if (targetTextObject == null || targetTextObject.Value == null)
            {
                return "Error: No text object selected";
            }
            
            if (stringVariable == null || stringVariable.Variable == null)
            {
                return "Error: No variable selected";
            }

            return targetTextObject.Value.name + " : " + stringVariable.Variable.Key;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(stringVariable.Variable, variable) || 
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        // Backwards compatibility with Fungus 3.x
        [HideInInspector]
        [FormerlySerializedAs("targetTextObject")]
        public GameObject _oldTargetText;
        protected override void OnEnable()
        {
            base.OnEnable();
            
        }

        protected override void EnsureLegacyVarIdsAreValid()
        {
            if (_oldStringVariable != null)
            {
                _oldStringVariable.ItemId = (byte)Mathf.Max(_oldStringVariable.ItemId, 1);
            }
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            EnsureLegacyVarIdsAreValid();

            if (_oldTargetText != null)
            {
                targetTextObject.Value = _oldTargetText;
            }

            if (_oldStringVariable != null)
            {
                stringVariable.Variable = _oldStringVariable;
                _oldStringVariable = null;
            }
        }   


        [FormerlySerializedAs("stringVariable")] [SerializeField] [HideInInspector] 
        protected StringVariable _oldStringVariable;

        #endregion
    }
}
