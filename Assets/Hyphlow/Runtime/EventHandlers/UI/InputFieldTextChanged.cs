using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [EventHandlerInfo("UI",
        "InputFieldTextChanged",
        "Executes this block when an input field's text changes.")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class InputFieldTextChanged : EventHandler
    {
        [SerializeField] [VariableProperty(typeof(GameObjectVariable))]
        protected GameObjectVariable inputFieldHolder;

        [SerializeField] [VariableProperty(typeof(StringVariable))]
        protected StringVariable output;

        protected override void Awake()
        {
            base.Awake();
            RegisterInputField();
        }

        protected virtual void RegisterInputField()
        {
            if (inputFieldHolder == null || inputFieldHolder.Value == null)
            {
                Debug.LogError($"[InputFieldTextChanged EventHandler]: Missing input field holder in Flowchart {_fChart.name}, Block {ParentBlock.BlockName}");
                return;
            }

            field = inputFieldHolder.Value.GetComponent<InputField>();
        }

        protected InputField field;

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            if (field == null)
            {
                return;
            }

            if (on)
            {
                field.onValueChanged.AddListener(OnTextChanged);
            }
            else
            {
                field.onValueChanged.RemoveListener(OnTextChanged);
            }
        }

        protected virtual void OnTextChanged(string newText)
        {
            output.Value = newText;
            ExecuteBlock();
        }

    }
}