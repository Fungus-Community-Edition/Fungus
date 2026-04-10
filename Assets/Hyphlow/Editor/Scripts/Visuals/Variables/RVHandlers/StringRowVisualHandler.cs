using UnityEngine.UIElements;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
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
            textValueField = ValueField as TextField;

            if (textValueField == null)
            {
                Debug.LogError($"StringRowVisualHandler could not find a TextField named in the UXML template. Check your UXML.");
                return;
            }

            textValueField.isDelayed = true; // This way, the change events only fire when the user presses enter
            textValueField.multiline = true;
            
        }

        protected TextField textValueField;
        protected override void ToggleValueChangeSubs(bool on)
        {
            base.ToggleValueChangeSubs(on);
            if (textValueField == null) return;
            if (on)
            {
                textValueField.RegisterValueChangedCallback(OnTextFieldChanged);
                textValueField.RegisterCallback<AttachToPanelEvent>(OnTextFieldAttachedToPanel); //
            }
            else
            {
                textValueField.UnregisterValueChangedCallback(OnTextFieldChanged);
                textValueField.UnregisterCallback<AttachToPanelEvent>(OnTextFieldAttachedToPanel); 
            }
        }

        protected virtual void OnTextFieldChanged(ChangeEvent<string> evt)
        {
            TriggerValueFieldChanged(evt.newValue);
        }

        protected virtual void OnTextFieldAttachedToPanel(AttachToPanelEvent evt)
        {
            ApplyVarValueToValueField();
        }

        protected override void ApplyVarValueToValueField()
        {
            textValueField.schedule.Execute(() =>
            {
                if (textValueField == null) return;
                string textToApply = (string)_currentVariable.BoxedValue;
                textValueField.SetValueWithoutNotify(textToApply);
                textValueField.MarkDirtyRepaint();
            }).ExecuteLater(1); // Delay by 1 frame to avoid UITK binding issues
        }

    }
}