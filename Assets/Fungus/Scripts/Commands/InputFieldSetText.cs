using UnityEngine;
using UnityEngine.UI;

namespace Fungus
{
    [CommandInfo("UI",
        "InputFieldSetText",
        "As it says on the tin.")]
    public class InputFieldSetText : Command
    {
        [SerializeField] protected GameObjectData inputFieldHolder;
        [SerializeField] protected StringData text;

        public override void OnEnter()
        {
            base.OnEnter();
            field = inputFieldHolder.Value.GetComponent<InputField>();
            field.SetTextWithoutNotify(text);
            Continue();
        }

        protected InputField field;

        public override string GetSummary()
        {
            if (!HasValidInputField)
            {
                return "ERROR: No Input field detected";
            }

            field = inputFieldHolder.Value.GetComponent<InputField>();
            string format = "Input field: " + field.name;
            string summary = string.Format(format, field.name);
            return summary;
        }

        protected bool HasValidInputField
        {
            get { return inputFieldHolder.Value != null && inputFieldHolder.Value.GetComponent<InputField>() != null; }
        }
    }
}