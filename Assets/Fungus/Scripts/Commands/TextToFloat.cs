using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fungus
{
    [CommandInfo("Variable",
        "Text To Float",
        "Takes the value of a Text field and applies the float version of it to a Float Variable")]
    public class TextToFloat : Command
    {
        [SerializeField] protected GameObjectData hasTextField;
        [SerializeField] [VariableProperty(typeof(FloatVariable))] protected FloatVariable output;

        public override void OnEnter()
        {
            base.OnEnter();
            if (hasTextField.Value != null)
            {
                legacyText = hasTextField.Value.GetComponent<Text>();
                tmproText = hasTextField.Value.GetComponent<TMP_Text>();
            }
            
            if (!HasValidTextField())
            {
                string format = "TextToFloat Command in {0}'s Flowchart: I ain't got no text field to work with! ;_;";
                string errorMessage = string.Format(format, this.gameObject.name);

                Debug.LogError(errorMessage);
                Continue();
                return;
            }

            string textToWorkWith = "";

            if (legacyText != null)
            {
                textToWorkWith = legacyText.text;
            }
            else if (tmproText != null)
            {
                textToWorkWith = tmproText.text;
            }

            float result;

            // Even InputFields that allow only numbers can have text that consists only of a period.
            if (textToWorkWith == ".")
            {
                result = 0;
            }
            else
            {
                bool parseSuccess = float.TryParse(textToWorkWith, out result);

                if (parseSuccess)
                {
                    output.Value = result;
                }
                else
                {
                    string format = "StringToFloat Command in Block {0} of {1}'s Flowchart: I can't convert the text '{2}' to a float! ;_;";
                    string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                        this.gameObject.name, textToWorkWith);
                    Debug.LogError(errorMessage);
                }
            }

            Continue();
        }

        protected Text legacyText;
        protected TMP_Text tmproText;

        protected virtual bool HasValidTextField()
        {
            return legacyText != null || tmproText != null;
        }

    }
}