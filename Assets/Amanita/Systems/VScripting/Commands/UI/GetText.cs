using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Amanita.VScripting
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
        [VariableProperty(typeof(StringVariable))]
        [SerializeField] protected StringVariable stringVariable;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(targetTextObject);
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
                stringVariable.Value = textAdapter.Text;
            }

            Continue();
        }
        
        public override string GetSummary()
        {
            if (targetTextObject == null)
            {
                return "Error: No text object selected";
            }
            
            if (stringVariable == null)
            {
                return "Error: No variable selected";
            }
            
            return targetTextObject.Value.name + " : " + stringVariable.name;
        }
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return stringVariable == variable || 
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        // Backwards compatibility with Fungus 3.x
        [HideInInspector]
        [FormerlySerializedAs("targetTextObject")]
        public GameObject targetTextObjectOLD;
        protected override void OnEnable()
        {
            base.OnEnable();
            if (targetTextObjectOLD != null)
            {
                targetTextObject.Value = targetTextObjectOLD.gameObject;
            }
        }

        #endregion
    }
}
