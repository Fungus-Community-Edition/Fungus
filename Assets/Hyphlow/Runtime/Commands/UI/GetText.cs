using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Gets the text property from a UI Text object and stores it in a string variable.
    /// </summary>
    [CommandInfo("UI", 
                 "Get Text", 
                 "Gets the text property from a UI Text object and stores it in a string variable.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class GetText : Command 
    {
        [Tooltip("Text object to get text value from")]
        [SerializeField] protected GameObjectData _targetTextObject = new GameObjectData();

        [Tooltip("String variable to store the text value in")]
        [ContentTypeConstraint(typeof(string))]
        [SerializeField] protected VariableReference stringVariable = new VariableReference();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetTextObject);
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
            textAdapter.InitFromGameObject(_targetTextObject);

            if (textAdapter.HasTextObject())
            {
                stringVariable.SetValue(textAdapter.Text);
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            if (_targetTextObject == null || _targetTextObject.Value == null)
            {
                return "Error: No text object selected";
            }
            
            if (stringVariable == null || stringVariable.Variable == null)
            {
                return "Error: No variable selected";
            }

            return _targetTextObject.Value.name + " : " + stringVariable.Variable.Key;
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
                _targetTextObject.Value = _oldTargetText;
                _oldTargetText = null;
            }

            if (_oldStringVariable != null)
            {
                stringVariable.Variable = _oldStringVariable;
                _oldStringVariable = null;
            }
        }

        [SerializeField] [HideInInspector] [FormerlySerializedAs("targetTextObject")]
        public GameObject _oldTargetText;


        [SerializeField] [HideInInspector] [FormerlySerializedAs("stringVariable")]  
        protected StringVariable _oldStringVariable;

        #endregion
    }
}
