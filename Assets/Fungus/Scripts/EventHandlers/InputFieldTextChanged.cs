using UnityEngine;
using UnityEngine.UI;

namespace Fungus
{
    [EventHandlerInfo("UI",
        "InputFieldTextChanged",
        "Executes this block when an input field's text changes.")]
    public class InputFieldTextChanged : EventHandler
    {
        [SerializeField] [VariableProperty(typeof(GameObjectVariable))]
        protected GameObjectVariable inputFieldHolder;

        [SerializeField] [VariableProperty(typeof(StringVariable))]
        protected StringVariable output;

        protected virtual void OnEnable()
        {
            InputField field = inputFieldHolder.Value.GetComponent<InputField>();
            field.onValueChanged.AddListener(OnTextChanged);
        }

        protected virtual void OnTextChanged(string newText)
        {
            output.Value = newText;
            ExecuteBlock();
        }

        protected virtual void OnDisable()
        {
            InputField field = inputFieldHolder.Value.GetComponent<InputField>();
            field.onValueChanged.RemoveListener(OnTextChanged);
        }
    }
}