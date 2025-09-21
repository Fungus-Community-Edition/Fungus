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
            textField = RowRoot.Q<TextField>("ValueField");
            textField.isDelayed = true; // This way, the change events only fire when the user presses enter
            textField.multiline = true;
            toRespondToFocusLoss.Add(textField);
        }

        protected TextField textField;

        protected override void ApplyDefaultBindingPaths()
        {
            base.ApplyDefaultBindingPaths();
            textField.bindingPath = "value";
        }

        protected override void ApplyMuscariableBindingPathOverrides()
        {
            base.ApplyMuscariableBindingPathOverrides();
            textField.bindingPath = $"{muscariableMemberName}.{textField.bindingPath}";
        }

        protected override void ToggleSubsForSignalingToTheOutside(bool on)
        {
            base.ToggleSubsForSignalingToTheOutside(on);
            if (textField == null)
            {
                return;
            }

            if (on)
            {
                textField.RegisterValueChangedCallback(OnTextFieldChanged);
            }
            else
            {
                textField.UnregisterValueChangedCallback(OnTextFieldChanged);
            }
        }

        protected virtual void OnTextFieldChanged(ChangeEvent<string> evt)
        {
            AmanitaEditorSignals.ControlValueChanged(evt);
            Debug.Log($"Triggered OnTextFieldChanged with new value {evt.newValue}");
        }

    }
}