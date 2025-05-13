using UnityEngine;

namespace Fungus
{
    [CommandInfo("Variable", 
        "StringToFloat",
        "Converts a string into a float")]
    public class StringToFloat : Command
    {
        [SerializeField] protected StringData input;
        [SerializeField] [VariableProperty(typeof(FloatVariable))]
        protected FloatVariable output;

        public override void OnEnter()
        {
            base.OnEnter();

            float result;
            string stringToWorkWith = input.Value;

            // Even InputFields that allow only numbers can have text that consists only of a period.
            if (stringToWorkWith == ".")
            {
                result = 0;
                output.Value = result;
            }
            else
            {
                
                bool parseSuccess = float.TryParse(stringToWorkWith, out result);

                if (parseSuccess)
                {
                    output.Value = result;
                }
                else
                {
                    string format = "StringToFloat Command in Block {0} of {1}'s Flowchart: input does not have a valid string to convert to a float.";
                    string errorMessage = string.Format(format, this.ParentBlock.BlockName,
                        this.gameObject.name);
                    Debug.LogError(errorMessage);
                }
            }

            Continue();
        }
    }
}