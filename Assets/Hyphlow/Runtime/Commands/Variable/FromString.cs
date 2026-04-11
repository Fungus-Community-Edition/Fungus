using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Attempts to parse a string into a given fungus variable type, such as integer or float
    /// </summary>
    [CommandInfo("Variable",
                 "From String",
                 "Attempts to parse a string into a given fungus variable type, such as integer or float")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class FromString : Command
    {
        [Tooltip("Source of string data to parse into another variables value")]
        [VariableProperty(typeof(StringVariable))]
        [SerializeField] protected StringVariable sourceString;

        [Tooltip("The variable type to be parsed and value stored within")]
        [VariableProperty(typeof(IntegerVariable), typeof(FloatVariable))]
        [SerializeField] protected Variable outValue;

        public override void OnEnter()
        {
            if (sourceString != null && outValue != null)
            {
                double asDouble = System.Convert.ToDouble(sourceString.Value, System.Globalization.CultureInfo.CurrentCulture);

                IntegerVariable intOutVar = outValue as IntegerVariable;
                if (intOutVar != null)
                {
                    intOutVar.Value = (int)asDouble;
                }
                else
                {
                    FloatVariable floatOutVar = outValue as FloatVariable;
                    if (floatOutVar != null)
                    {
                        floatOutVar.Value = (float)asDouble;
                    }
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (sourceString == null)
            {
                return "Error: No source string selected";
            }

            if (outValue == null)
            {
                return "Error: No type and storage variable selected";
            }

            return outValue.Key + ".Parse " + sourceString.Key;
        }

        public override bool HasReference(Variable variable)
        {
            return (variable == sourceString) || (variable == outValue);
        }

        public override Color GetButtonColor()
        {
            return new Color32(253, 253, 150, 255);
        }
    }
}
