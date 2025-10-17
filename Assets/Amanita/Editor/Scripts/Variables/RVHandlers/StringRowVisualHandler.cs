using Amanita.EditorUtils;
using UnityEngine.UIElements;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler(menuName: "Graphics",
        contentType: typeof(string),
        typeDisplayName: "String",
        pathToTemplate: "UIToolkitTemplates/VarRows/StringVariableRow")]
    public class StringRowVisualHandler : RowVisualHandler<object>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            textValueField = valueField as TextField;

            if (textValueField == null)
            {
                Debug.LogError($"StringRowVisualHandler could not find a TextField named in the UXML template. Check your UXML.");
                return;
            }

            textValueField.isDelayed = true; // This way, the change events only fire when the user presses enter
            textValueField.multiline = true;
            
        }

        protected TextField textValueField;
        protected override void ToggleSpecificFieldSubs(bool on)
        {
            base.ToggleSpecificFieldSubs(on);
            if (on)
            {
                textValueField.RegisterValueChangedCallback(OnTextFieldChanged);
            }
            else
            {
                textValueField.UnregisterValueChangedCallback(OnTextFieldChanged);
            }
        }

        protected virtual void OnTextFieldChanged(ChangeEvent<string> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
            AmanitaEditorSignals.ControlValueChanged(evt);
        }

        protected override void BindForMuscarisInFlowcharts()
        {
            base.BindForMuscarisInFlowcharts();
            textValueField.value = _currentVariable.Value as string;
        }

    }
}