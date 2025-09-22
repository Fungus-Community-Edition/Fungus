using Amanita.EditorUtils;
using UnityEngine.UIElements;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Primitives", typeof(string), "String",
        "UIToolkitTemplates/VarRows/StringVariableRow")]
    public class StringRowVisualHandler : RowVisualHandler<object>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            textField = valueField as TextField;

            if (textField == null)
            {
                Debug.LogError($"StringRowVisualHandler could not find a TextField named in the UXML template. Check your UXML.");
                return;
            }

            textField.isDelayed = true; // This way, the change events only fire when the user presses enter
            textField.multiline = true;
        }

        protected TextField textField;
        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            ToggleValueChange(textField, OnTextFieldChanged, on);
        }

        protected virtual void OnTextFieldChanged(ChangeEvent<string> evt)
        {
            AmanitaEditorSignals.ControlValueChanged(evt);
        }

    }
}