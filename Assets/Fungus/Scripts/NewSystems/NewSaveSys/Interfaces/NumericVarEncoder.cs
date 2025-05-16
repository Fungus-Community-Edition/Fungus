using Fungus;
using System;

namespace Amanita.SaveSys
{
    public class NumericVarEncoder : IVarEncoder
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is IntegerVariable || variable is FloatVariable;

        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(IntegerVariable) || typeName == nameof(FloatVariable);

        public virtual string Encode(Variable variable) => variable switch
        {
            IntegerVariable intVar => intVar.Value.ToString(),
            FloatVariable floatVar => floatVar.Value.ToString(roundTripFormat),
            _ => throw new InvalidOperationException()
        };

        protected static string roundTripFormat = "R";
        // ^ This is to make sure that when we convert a float to a string and then
        // back to a float, we get the exact same value.
        // We want to decode things as accurately as possible, so...

        public virtual void Decode(Variable variable, string data)
        {
            if (variable is IntegerVariable intVar)
                intVar.Value = int.Parse(data);
            else if (variable is FloatVariable floatVar)
                floatVar.Value = float.Parse(data);
        }
    }
}